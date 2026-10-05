namespace loja_s.Models;

public class SegurancaConta
{
    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public int TentativasFalhas { get; set; }

    public DateTime? BloqueadoAte { get; set; }

    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
}
