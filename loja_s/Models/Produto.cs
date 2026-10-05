namespace loja_s.Models;

public class Produto
{
    public string Nome { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public string? ImagemUrl { get; set; }
}
