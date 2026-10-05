using System.ComponentModel.DataAnnotations;

namespace bow.estoque.ViewModels;

public class VendaCreateViewModel
{
    [Required]
    public int ClienteId { get; set; }
    public int UsuarioId { get; set; }
    [Range(0, 999999)] public decimal Desconto { get; set; }
    [Range(0, 999999)] public decimal Frete { get; set; }
    public List<VendaItemForm> Itens { get; set; } = new();
}

public class VendaItemForm
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
}
