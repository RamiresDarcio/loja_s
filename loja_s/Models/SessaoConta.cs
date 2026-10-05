namespace loja_s.Models;

public class SessaoConta
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public string ChaveSessao { get; set; } = string.Empty;

    public string Dispositivo { get; set; } = string.Empty;

    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public DateTime UltimaAtividade { get; set; } = DateTime.UtcNow;
}
