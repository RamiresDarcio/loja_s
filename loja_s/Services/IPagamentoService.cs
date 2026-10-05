using loja_s.Models;

namespace loja_s.Services;

public interface IPagamentoService
{
    Task<Pagamento> ProcessarAsync(Pedido pedido, string formaPagamento, string? dadosExtras = null);
}
