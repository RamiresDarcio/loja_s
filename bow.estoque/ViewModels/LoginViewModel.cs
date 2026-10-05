using System.ComponentModel.DataAnnotations;

namespace bow.estoque.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o usuário.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;
}
