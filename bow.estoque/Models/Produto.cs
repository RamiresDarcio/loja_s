using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bow.estoque.Models;

public class Produto
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string Nome { get; set; } = string.Empty;
    [StringLength(1000)] public string Descricao { get; set; } = string.Empty;
    [Required, StringLength(50)] public string SKU { get; set; } = string.Empty;
    [StringLength(50)] public string CodigoBarras { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public int FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PrecoCusto { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PrecoVenda { get; set; }
    public int QuantidadeEstoque { get; set; }
    public int EstoqueMinimo { get; set; }
    public int EstoqueMaximo { get; set; }
    [StringLength(300)] public string Imagem { get; set; } = string.Empty;
    [StringLength(20)] public string Status { get; set; } = "Ativo";
    public DateTime DataCadastro { get; set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;
    public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = new List<MovimentacaoEstoque>();
}
