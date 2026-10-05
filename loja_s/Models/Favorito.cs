namespace loja_s.Models;

public class Favorito
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public int ProdutoId { get; set; }

    public Produto Produto { get; set; } = null!;

    public DateTime DataAdicionado { get; set; } = DateTime.UtcNow;
}
