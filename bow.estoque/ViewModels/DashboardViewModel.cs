namespace bow.estoque.ViewModels;

public class DashboardViewModel
{
    public int TotalProdutos { get; set; }
    public int ProdutosEmEstoque { get; set; }
    public int EstoqueBaixo { get; set; }
    public int ProdutosSemEstoque { get; set; }
    public int TotalVendas { get; set; }
    public decimal Faturamento { get; set; }
    public List<string> VendasPorMes { get; set; } = new();
    public List<int> QuantidadeVendasPorMes { get; set; } = new();
    public List<string> ProdutosMaisVendidos { get; set; } = new();
    public List<int> QuantidadeVendidaPorProduto { get; set; } = new();
    public List<string> ProdutosEstoque { get; set; } = new();
    public List<int> QuantidadeEstoqueAtual { get; set; } = new();
}

public class DashboardMonthlySales
{
    public string Mes { get; set; } = string.Empty;
    public int Total { get; set; }
}

public class DashboardProductSales
{
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}

public class DashboardStoreOrderValue
{
    public decimal ValorTotal { get; set; }
}
