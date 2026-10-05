namespace loja_s.Models;

public class SolicitacaoSuporte
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public string Categoria { get; set; } = string.Empty;

    public string Assunto { get; set; } = string.Empty;

    public string Mensagem { get; set; } = string.Empty;

    public string Status { get; set; } = "Aberta";

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
