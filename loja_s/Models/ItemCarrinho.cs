namespace loja_s.Models;

public class ItemCarrinho
{
    public int Id { get; set; }

    public int CarrinhoId { get; set; }

    public Carrinho? Carrinho { get; set; }

    public int ProdutoId { get; set; }

    public string NomeProduto { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public decimal PrecoUnitario { get; set; }

    public decimal Subtotal => Quantidade * PrecoUnitario;
}
