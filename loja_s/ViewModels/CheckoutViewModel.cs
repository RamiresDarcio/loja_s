using System.ComponentModel.DataAnnotations;

namespace loja_s.ViewModels;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Informe seu nome completo.")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CPF.")]
    public string CPF { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o telefone.")]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CEP.")]
    public string CEP { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a rua.")]
    public string Rua { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o número.")]
    public string Numero { get; set; } = string.Empty;

    public string? Complemento { get; set; }

    [Required(ErrorMessage = "Informe o bairro.")]
    public string Bairro { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a cidade.")]
    public string Cidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o estado.")]
    public string Estado { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione uma forma de pagamento.")]
    public string FormaPagamento { get; set; } = "PIX";

    [TrueRequired(ErrorMessage = "Você precisa confirmar os termos.")]
    public bool AceitaTermos { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Frete { get; set; } = 19.90m;
    public decimal Desconto { get; set; }
    public decimal ValorTotal { get; set; }
    public List<ItemCarrinhoResumo> Itens { get; set; } = new();
}

public class ItemCarrinhoResumo
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
