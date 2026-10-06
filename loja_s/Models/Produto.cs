using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace loja_s.Models;

public class Produto
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public decimal? PrecoPromocional { get; set; }

    public decimal PrecoCusto { get; set; }

    public int Estoque { get; set; }

    public int EstoqueMinimo { get; set; }

    public int EstoqueMaximo { get; set; }

    public int? CategoriaId { get; set; }

    public int? FornecedorId { get; set; }

    [StringLength(50)]
    public string CodigoBarras { get; set; } = string.Empty;

    [StringLength(100)]
    public string Marca { get; set; } = string.Empty;

    [StringLength(100)]
    public string? EdicaoEspecial { get; set; }

    [StringLength(1000)]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(300)]
    public string ImagemUrl { get; set; } = string.Empty;

    [StringLength(20)]
    public string Status { get; set; } = "Ativo";

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public decimal PrecoEfetivo =>
        PrecoPromocional is > 0 && PrecoPromocional < Preco ? PrecoPromocional.Value : Preco;
}
