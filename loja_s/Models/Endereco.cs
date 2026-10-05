using System.ComponentModel.DataAnnotations;

namespace loja_s.Models;

public class Endereco
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Nome { get; set; } = "Endereço principal";

    [Required]
    public string CEP { get; set; } = string.Empty;

    [Required]
    public string Rua { get; set; } = string.Empty;

    [Required]
    public string Numero { get; set; } = string.Empty;

    public string? Complemento { get; set; }

    [Required]
    public string Bairro { get; set; } = string.Empty;

    [Required]
    public string Cidade { get; set; } = string.Empty;

    [Required]
    public string Estado { get; set; } = string.Empty;

    public string Pais { get; set; } = "Brasil";

    public bool Principal { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}
