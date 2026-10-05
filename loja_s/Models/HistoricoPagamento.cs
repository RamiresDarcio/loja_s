namespace loja_s.Models;

public class HistoricoPagamento
{
    public int Id { get; set; }

    public int PedidoId { get; set; }

    public string StatusAnterior { get; set; } = string.Empty;

    public string StatusAtual { get; set; } = string.Empty;

    public string Observacao { get; set; } = string.Empty;

    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
}
