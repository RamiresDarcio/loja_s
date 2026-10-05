using System.ComponentModel.DataAnnotations;

namespace loja_s.ViewModels;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class TrueRequiredAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is true;
}
