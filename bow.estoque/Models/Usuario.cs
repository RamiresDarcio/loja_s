using System.ComponentModel.DataAnnotations;

namespace bow.estoque.Models;

public class Usuario
{
    public int Id { get; set; }
    [Required, StringLength(50)] public string NomeUsuario { get; set; } = string.Empty;
    [Required, StringLength(150)] public string NomeCompleto { get; set; } = string.Empty;
    [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string SenhaHash { get; set; } = string.Empty;
    [StringLength(20)] public string Status { get; set; } = "Ativo";
    public DateTime DataCadastro { get; set; } = DateTime.Now;
}
