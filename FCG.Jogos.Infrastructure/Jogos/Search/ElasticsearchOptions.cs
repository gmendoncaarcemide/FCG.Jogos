namespace FCG.Jogos.Infrastructure.Jogos.Search;

public class ElasticsearchOptions
{
    public bool Enable { get; set; } = true;
    public string? Node { get; set; } // e.g., http://localhost:9200
    public string? CloudId { get; set; } // Elastic Cloud (optional)
    public string? ApiKey { get; set; } // base64 id:apiKey or raw, as supported
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string IndexName { get; set; } = "jogos";
}
