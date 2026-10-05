namespace loja_s.Models;

public class Produto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public int Estoque { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public string Status { get; set; } = "Ativo";

    public string? ImagemUrl { get; set; }

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}
