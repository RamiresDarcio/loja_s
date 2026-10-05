using System.ComponentModel.DataAnnotations;

namespace bow.estoque.Models;

public class Categoria
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nome { get; set; } = string.Empty;
    [StringLength(500)] public string Descricao { get; set; } = string.Empty;
    [StringLength(20)] public string Status { get; set; } = "Ativo";
    public DateTime DataCadastro { get; set; } = DateTime.Now;
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
