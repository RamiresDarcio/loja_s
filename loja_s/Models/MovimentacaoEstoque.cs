namespace loja_s.Models;

public class MovimentacaoEstoque
{
    public int Id { get; set; }

    public int ProdutoId { get; set; }

    public Produto? Produto { get; set; }

    public string TipoMovimentacao { get; set; } = "Entrada";

    public int Quantidade { get; set; }

    public int EstoqueAnterior { get; set; }

    public int EstoqueAtual { get; set; }

    public string Motivo { get; set; } = string.Empty;

    public int UsuarioId { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;
}
