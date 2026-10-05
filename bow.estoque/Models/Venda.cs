using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bow.estoque.Models;

public class Venda
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public DateTime Data { get; set; } = DateTime.Now;
    [StringLength(20)] public string Status { get; set; } = "Pendente";
    [Column(TypeName = "decimal(18,2)")] public decimal Subtotal { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Desconto { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Frete { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Total { get; set; }
    public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
}
