namespace loja_s.Models;

public class TokenConta
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public string Finalidade { get; set; } = string.Empty;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiraEm { get; set; }

    public bool Utilizado { get; set; }
}
