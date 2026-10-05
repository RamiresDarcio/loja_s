namespace loja_s.Models;

public class Pedido
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }

    public string NumeroPedido { get; set; } = string.Empty;

    public DateTime DataPedido { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Pendente";

    public decimal Subtotal { get; set; }

    public decimal Frete { get; set; }

    public decimal Desconto { get; set; }

    public decimal ValorTotal { get; set; }

    public string FormaPagamento { get; set; } = "PIX";

    public string StatusPagamento { get; set; } = "Pendente";

    public int EnderecoEntregaId { get; set; }

    public Endereco? EnderecoEntrega { get; set; }

    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();

    public Pagamento? Pagamento { get; set; }
}
