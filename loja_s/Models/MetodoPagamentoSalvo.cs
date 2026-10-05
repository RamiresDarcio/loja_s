namespace loja_s.Models;

public class MetodoPagamentoSalvo
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public string Bandeira { get; set; } = string.Empty;

    public string UltimosQuatro { get; set; } = string.Empty;

    public string TokenProvedor { get; set; } = string.Empty;

    public bool Preferencial { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}
