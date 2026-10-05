using System.ComponentModel.DataAnnotations.Schema;

namespace bow.estoque.Models;

public class ItemVenda
{
    public int Id { get; set; }
    public int VendaId { get; set; }
    public Venda? Venda { get; set; }
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }
    public int Quantidade { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PrecoUnitario { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Subtotal { get; set; }
}
