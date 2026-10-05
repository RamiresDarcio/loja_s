using System.ComponentModel.DataAnnotations;

namespace bow.estoque.Models;

public class Fornecedor
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string RazaoSocial { get; set; } = string.Empty;
    [StringLength(200)] public string NomeFantasia { get; set; } = string.Empty;
    [Required, StringLength(20)] public string CNPJ { get; set; } = string.Empty;
    [EmailAddress] public string Email { get; set; } = string.Empty;
    [Phone] public string Telefone { get; set; } = string.Empty;
    [StringLength(20)] public string CEP { get; set; } = string.Empty;
    [StringLength(100)] public string Estado { get; set; } = string.Empty;
    [StringLength(150)] public string Cidade { get; set; } = string.Empty;
    [StringLength(250)] public string Endereco { get; set; } = string.Empty;
    [StringLength(20)] public string Numero { get; set; } = string.Empty;
    [StringLength(200)] public string Complemento { get; set; } = string.Empty;
    [StringLength(20)] public string Status { get; set; } = "Ativo";
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
