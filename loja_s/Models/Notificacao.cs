namespace loja_s.Models;

public class Notificacao
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public string Categoria { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string Mensagem { get; set; } = string.Empty;

    public string? Url { get; set; }

    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public DateTime? LidaEm { get; set; }
}
