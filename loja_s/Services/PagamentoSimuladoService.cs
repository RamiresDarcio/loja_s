using loja_s.Data;
using loja_s.Models;

namespace loja_s.Services;

public class PagamentoSimuladoService : IPagamentoService
{
    private readonly ApplicationDbContext _context;

    public PagamentoSimuladoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Pagamento> ProcessarAsync(Pedido pedido, string formaPagamento, string? dadosExtras = null)
    {
        var token = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();

        var pagamento = new Pagamento
        {
            PedidoId = pedido.Id,
            FormaPagamento = formaPagamento,
            Valor = pedido.ValorTotal,
            Gateway = "Simulado",
            CodigoReferencia = token,
            DataPagamento = DateTime.UtcNow,
            Status = "Pendente"
        };

        if (formaPagamento.Equals("CartaoCredito", StringComparison.OrdinalIgnoreCase))
        {
            pagamento.Status = "Pago";
            pagamento.TokenCartao = "tok_simulado_" + Guid.NewGuid().ToString("N")[..16];
        }
        else if (formaPagamento.Equals("PIX", StringComparison.OrdinalIgnoreCase))
        {
            pagamento.Status = "AguardandoPagamento";
            pagamento.CodigoReferencia = "pix_" + token;
        }
        else if (formaPagamento.Equals("Boleto", StringComparison.OrdinalIgnoreCase))
        {
            pagamento.Status = "AguardandoPagamento";
            pagamento.CodigoReferencia = "boleto_" + token;
        }
        else
        {
            pagamento.Status = "Pendente";
        }

        _context.Pagamentos.Add(pagamento);
        await _context.SaveChangesAsync();

        pedido.StatusPagamento = pagamento.Status;
        pedido.FormaPagamento = formaPagamento;

        if (pagamento.Status == "Pago")
        {
            pedido.Status = "Finalizado";
        }

        return pagamento;
    }
}
