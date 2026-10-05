namespace loja_s.Models;

public class PerfilConta
{
    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public string Sobrenome { get; set; } = string.Empty;

    public DateTime? DataNascimento { get; set; }

    public DateTime? TermosAceitosEm { get; set; }

    public bool EmailConfirmado { get; set; }

    public bool AtualizacoesPedidos { get; set; } = true;

    public bool Promocoes { get; set; }

    public bool Novidades { get; set; }

    public bool ProdutosFavoritos { get; set; } = true;

    public bool AlertasSeguranca { get; set; } = true;
}
