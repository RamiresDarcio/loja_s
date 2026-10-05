namespace loja_s.Models;

public class Pagamento
{
    public int Id { get; set; }

    public int PedidoId { get; set; }

    public Pedido Pedido { get; set; } = null!;

    public string FormaPagamento { get; set; } = "PIX";

    public string Status { get; set; } = "Pendente";

    public decimal Valor { get; set; }

    public string CodigoReferencia { get; set; } = string.Empty;

    public string? Gateway { get; set; }

    public string? TokenCartao { get; set; }

    public DateTime DataPagamento { get; set; } = DateTime.UtcNow;
}
