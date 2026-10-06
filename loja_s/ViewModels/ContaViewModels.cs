using System.ComponentModel.DataAnnotations;
using loja_s.Models;

namespace loja_s.ViewModels;

public class CadastroContaViewModel
{
    public string? ReturnUrl { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Sobrenome")]
    public string Sobrenome { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Data de nascimento")]
    public DateTime? DataNascimento { get; set; }

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 10)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).+$",
        ErrorMessage = "Use pelo menos 10 caracteres, com maiúscula, minúscula, número e símbolo.")]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Senha))]
    [Display(Name = "Confirmar senha")]
    public string ConfirmarSenha { get; set; } = string.Empty;

    [TrueRequired(ErrorMessage = "É necessário aceitar os termos de uso.")]
    [Display(Name = "Aceito os termos de uso e a política de privacidade")]
    public bool AceitouTermos { get; set; }
}

public class LoginContaViewModel
{
    [Required, EmailAddress]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Display(Name = "Manter conectado")]
    public bool ManterConectado { get; set; }

    public string? ReturnUrl { get; set; }
}

public class DadosContaViewModel
{
    [Required, StringLength(80)]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Sobrenome")]
    public string Sobrenome { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Data de nascimento")]
    public DateTime? DataNascimento { get; set; }

    [Display(Name = "CPF")]
    public string CPF { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Senha atual (necessária para alterar e-mail)")]
    public string? SenhaAtual { get; set; }
}

public class EnderecoContaViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Nome do destinatário")]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(12)]
    [Display(Name = "CEP")]
    public string CEP { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Rua")]
    public string Rua { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Display(Name = "Número")]
    public string Numero { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Complemento { get; set; }

    [Required, StringLength(100)]
    public string Bairro { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [Required, StringLength(2, MinimumLength = 2)]
    [Display(Name = "Estado (UF)")]
    public string Estado { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Pais { get; set; } = "Brasil";

    [Display(Name = "Definir como endereço principal")]
    public bool Principal { get; set; }
}

public class RecuperarSenhaViewModel
{
    [Required, EmailAddress]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;
}

public class RedefinirSenhaViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 10)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).+$",
        ErrorMessage = "Use pelo menos 10 caracteres, com maiúscula, minúscula, número e símbolo.")]
    [Display(Name = "Nova senha")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(NovaSenha))]
    [Display(Name = "Confirmar nova senha")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}

public class AlterarSenhaViewModel
{
    [Required, DataType(DataType.Password)]
    [Display(Name = "Senha atual")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 10)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).+$",
        ErrorMessage = "Use pelo menos 10 caracteres, com maiúscula, minúscula, número e símbolo.")]
    [Display(Name = "Nova senha")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(NovaSenha))]
    [Display(Name = "Confirmar nova senha")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}

public class MetodoPagamentoViewModel
{
    [Required, StringLength(30)]
    [Display(Name = "Bandeira")]
    public string Bandeira { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{4}$")]
    [Display(Name = "Quatro últimos dígitos")]
    public string UltimosQuatro { get; set; } = string.Empty;

    [Required, StringLength(250)]
    [Display(Name = "Token fornecido pelo provedor de pagamentos")]
    public string TokenProvedor { get; set; } = string.Empty;
}

public class SuporteContaViewModel
{
    [Required]
    [Display(Name = "Categoria")]
    public string Categoria { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Assunto { get; set; } = string.Empty;

    [Required, StringLength(3000, MinimumLength = 10)]
    public string Mensagem { get; set; } = string.Empty;
}

public class PreferenciasNotificacaoViewModel
{
    [Display(Name = "Atualizações de pedidos")]
    public bool AtualizacoesPedidos { get; set; }

    [Display(Name = "Promoções")]
    public bool Promocoes { get; set; }

    [Display(Name = "Novidades")]
    public bool Novidades { get; set; }

    [Display(Name = "Produtos favoritos")]
    public bool ProdutosFavoritos { get; set; }

    [Display(Name = "Alertas de segurança")]
    public bool AlertasSeguranca { get; set; }

    public List<Notificacao> Notificacoes { get; set; } = [];
}
