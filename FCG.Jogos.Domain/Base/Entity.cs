namespace FCG.Jogos.Domain.Base;

public abstract class Entity
{
    public Guid Id { get; set; }
    public DateTimeOffset DataCriacao { get; set; }
    public DateTimeOffset? DataAtualizacao { get; set; }
    public bool Ativo { get; set; } = true;

    protected Entity()
    {
        Id = Guid.NewGuid();
        DataCriacao = DateTimeOffset.UtcNow;
    }
}