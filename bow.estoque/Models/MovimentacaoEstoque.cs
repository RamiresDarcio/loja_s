using System.ComponentModel.DataAnnotations;

namespace bow.estoque.Models;

public class MovimentacaoEstoque
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }
    [Required, StringLength(30)] public string TipoMovimentacao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public int EstoqueAnterior { get; set; }
    public int EstoqueAtual { get; set; }
    [StringLength(500)] public string Motivo { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public DateTime Data { get; set; } = DateTime.Now;
}
