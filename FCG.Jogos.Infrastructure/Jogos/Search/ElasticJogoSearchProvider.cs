using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FCG.Jogos.Domain.Jogos.Entities;
using FCG.Jogos.Domain.Jogos.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FCG.Jogos.Infrastructure.Jogos.Search;

public class ElasticJogoSearchProvider : IJogoSearchProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ElasticsearchOptions _options;
    private readonly ILogger<ElasticJogoSearchProvider> _logger;
    private readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public ElasticJogoSearchProvider(IHttpClientFactory httpClientFactory, IOptions<ElasticsearchOptions> options, ILogger<ElasticJogoSearchProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    private string BaseUrl => ResolveBaseUrl();
    private string Index => string.IsNullOrWhiteSpace(_options.IndexName) ? "jogos" : _options.IndexName;

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(BaseUrl.TrimEnd('/') + "/");
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApiKey", _options.ApiKey);
        }
        else if (!string.IsNullOrWhiteSpace(_options.Username) && !string.IsNullOrWhiteSpace(_options.Password))
        {
            var raw = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", raw);
        }
        return client;
    }

    private string ResolveBaseUrl()
    {
        // 1) If CloudId is provided (Elastic Cloud), derive the HTTPS endpoint.
        // Cloud ID format: "<deployment_name>:<base64 of 'es_uuid:es_hostname$kb_uuid:kb_hostname'>"
        // We need the es_hostname, and we will use https://<es_hostname>
        if (!string.IsNullOrWhiteSpace(_options.CloudId))
        {
            try
            {
                var parts = _options.CloudId!.Split(':', 2);
                if (parts.Length == 2)
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
                    // decoded example: "es_uuid:abcdefg.us-central1.gcp.elastic-cloud.com$kb_uuid:..."
                    var first = decoded.Split('$')[0];
                    var esTokens = first.Split(':');
                    if (esTokens.Length >= 2)
                    {
                        var host = esTokens[1];
                        if (!string.IsNullOrWhiteSpace(host))
                        {
                            return $"https://{host}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse Elasticsearch CloudId. Falling back to localhost:9200");
            }
        }

        // 2) If Node is explicitly set, honor it.
        if (!string.IsNullOrWhiteSpace(_options.Node))
        {
            return _options.Node!;
        }

        // 3) Fallback to localhost (dev only)
        return "http://localhost:9200";
    }

    public async Task IndexAsync(Jogo jogo, CancellationToken ct = default)
    {
        await EnsureIndexAsync(ct);
        var client = CreateClient();
        var json = JsonSerializer.Serialize(jogo, _jsonOpts);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await client.PutAsync($"{Index}/_doc/{jogo.Id}", content, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Elasticsearch index failed: {Status} {Body}", resp.StatusCode, body);
        }
    }

    public async Task DeleteAsync(Guid jogoId, CancellationToken ct = default)
    {
        await EnsureIndexAsync(ct);
        var client = CreateClient();
        var resp = await client.DeleteAsync($"{Index}/_doc/{jogoId}", ct);
        if (!resp.IsSuccessStatusCode && resp.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Elasticsearch delete failed: {Status} {Body}", resp.StatusCode, body);
        }
    }

    public async Task<IReadOnlyCollection<Jogo>> SearchAsync(string? query, string? categoria, decimal? precoMin, decimal? precoMax, string[]? tags, int page, int pageSize, CancellationToken ct = default)
    {
        await EnsureIndexAsync(ct);
        var client = CreateClient();

        var must = new List<object>();
        if (!string.IsNullOrWhiteSpace(query))
        {
            must.Add(new
            {
                multi_match = new
                {
                    query,
                    fields = new[] { "titulo^3", "descricao", "tags^2", "plataformas", "desenvolvedor", "editora" },
                    fuzziness = "AUTO"
                }
            });
        }
        if (!string.IsNullOrWhiteSpace(categoria) && int.TryParse(categoria, out var catInt))
        {
            must.Add(new { term = new { categoria = catInt } });
        }
        if (precoMin.HasValue || precoMax.HasValue)
        {
            must.Add(new { range = new { preco = new { gte = precoMin, lte = precoMax } } });
        }
        if (tags != null && tags.Length > 0)
        {
            must.Add(new { terms = new { tags } });
        }

        var from = Math.Max(0, (page - 1) * pageSize);
        var body = new
        {
            from,
            size = pageSize,
            query = new { @bool = new { must } }
        };

        var req = new StringContent(JsonSerializer.Serialize(body, _jsonOpts), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync($"{Index}/_search", req, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Elasticsearch search failed: {Status} {Body}", resp.StatusCode, err);
            return Array.Empty<Jogo>();
        }

        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var hits = doc.RootElement.GetProperty("hits").GetProperty("hits");
        var list = new List<Jogo>();
        foreach (var hit in hits.EnumerateArray())
        {
            if (hit.TryGetProperty("_source", out var src))
            {
                try
                {
                    var jogo = src.Deserialize<Jogo>(_jsonOpts);
                    if (jogo != null) list.Add(jogo);
                }
                catch { }
            }
        }
        return list;
    }

    public async Task<IReadOnlyCollection<Jogo>> SuggestForUserAsync(Guid usuarioId, int quantidade = 10, CancellationToken ct = default)
    {
        await EnsureIndexAsync(ct);
        var client = CreateClient();
        var body = new
        {
            size = quantidade,
            sort = new object[] { new { avaliacaoMedia = new { order = "desc" } } },
            query = new { match_all = new { } }
        };
        var req = new StringContent(JsonSerializer.Serialize(body, _jsonOpts), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync($"{Index}/_search", req, ct);
        if (!resp.IsSuccessStatusCode)
        {
            return Array.Empty<Jogo>();
        }
        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var hits = doc.RootElement.GetProperty("hits").GetProperty("hits");
        var list = new List<Jogo>();
        foreach (var hit in hits.EnumerateArray())
        {
            if (hit.TryGetProperty("_source", out var src))
            {
                var jogo = src.Deserialize<Jogo>(_jsonOpts);
                if (jogo != null) list.Add(jogo);
            }
        }
        return list;
    }

    public async Task<PopularMetricsResponse> GetPopularMetricsAsync(int top = 10, CancellationToken ct = default)
    {
        await EnsureIndexAsync(ct);
        var client = CreateClient();
        var body = new
        {
            size = 0,
            aggs = new
            {
                top_tags = new { terms = new { field = "tags", size = top } },
                top_plataformas = new { terms = new { field = "plataformas", size = top } },
                top_categorias = new { terms = new { field = "categoria", size = top } }
            }
        };
        var req = new StringContent(JsonSerializer.Serialize(body, _jsonOpts), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync($"{Index}/_search", req, ct);
        if (!resp.IsSuccessStatusCode)
        {
            return new PopularMetricsResponse
            {
                TopTags = Array.Empty<string>(),
                TopPlataformas = Array.Empty<string>(),
                TopCategorias = Array.Empty<string>()
            };
        }
        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var aggs = doc.RootElement.GetProperty("aggregations");

        IReadOnlyCollection<string> ReadBuckets(string name)
        {
            if (!aggs.TryGetProperty(name, out var a)) return Array.Empty<string>();
            if (!a.TryGetProperty("buckets", out var buckets)) return Array.Empty<string>();
            var list = new List<string>();
            foreach (var b in buckets.EnumerateArray())
            {
                if (b.TryGetProperty("key", out var key)) list.Add(key.ToString());
            }
            return list;
        }

        return new PopularMetricsResponse
        {
            TopTags = ReadBuckets("top_tags"),
            TopPlataformas = ReadBuckets("top_plataformas"),
            TopCategorias = ReadBuckets("top_categorias")
        };
    }

    private async Task EnsureIndexAsync(CancellationToken ct)
    {
        var client = CreateClient();
        var head = new HttpRequestMessage(HttpMethod.Head, Index);
        var headResp = await client.SendAsync(head, ct);
        if (headResp.IsSuccessStatusCode)
            return; // exists

        var mappings = new
        {
            mappings = new
            {
                properties = new Dictionary<string, object>
                {
                    ["titulo"] = new { type = "text" },
                    ["descricao"] = new { type = "text" },
                    ["desenvolvedor"] = new { type = "keyword" },
                    ["editora"] = new { type = "keyword" },
                    ["dataLancamento"] = new { type = "date" },
                    ["preco"] = new { type = "double" },
                    ["tags"] = new { type = "keyword" },
                    ["plataformas"] = new { type = "keyword" },
                    ["categoria"] = new { type = "integer" },
                    ["classificacao"] = new { type = "integer" },
                    ["avaliacaoMedia"] = new { type = "integer" },
                    ["numeroAvaliacoes"] = new { type = "integer" },
                    ["disponivel"] = new { type = "boolean" },
                    ["estoque"] = new { type = "integer" }
                }
            }
        };
        var req = new StringContent(JsonSerializer.Serialize(mappings, _jsonOpts), Encoding.UTF8, "application/json");
        var createResp = await client.PutAsync(Index, req, ct);
        if (!createResp.IsSuccessStatusCode)
        {
            var body = await createResp.Content.ReadAsStringAsync(ct);
            throw new Exception($"Failed to create Elasticsearch index '{Index}': {createResp.StatusCode} {body}");
        }
    }
}
