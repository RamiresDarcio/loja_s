namespace loja_s.Models;

public class Carrinho
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public List<ItemCarrinho> Itens { get; set; } = new();
}
