using System.ComponentModel.DataAnnotations;

namespace loja_s.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Telefone { get; set; }

    public string CPF { get; set; } = string.Empty;

    public string SenhaHash { get; set; } = string.Empty;

    public string Tipo { get; set; } = "Cliente";

    public string Status { get; set; } = "Ativo";

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public PerfilConta? PerfilConta { get; set; }

    public SegurancaConta? SegurancaConta { get; set; }
}
