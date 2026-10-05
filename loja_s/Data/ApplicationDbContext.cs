using loja_s.Models;
using Microsoft.EntityFrameworkCore;

namespace loja_s.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Carrinho> Carrinhos { get; set; }
    public DbSet<ItemCarrinho> ItensCarrinho { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<ItemPedido> ItensPedido { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }
    public DbSet<Endereco> Enderecos { get; set; }
    public DbSet<HistoricoPagamento> HistoricoPagamentos { get; set; }
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
    public DbSet<PerfilConta> PerfisConta { get; set; }
    public DbSet<SegurancaConta> SegurancasConta { get; set; }
    public DbSet<Favorito> Favoritos { get; set; }
    public DbSet<MetodoPagamentoSalvo> MetodosPagamentoSalvos { get; set; }
    public DbSet<SessaoConta> SessoesConta { get; set; }
    public DbSet<TokenConta> TokensConta { get; set; }
    public DbSet<SolicitacaoSuporte> SolicitacoesSuporte { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Produto>().HasIndex(p => p.SKU).IsUnique();
        modelBuilder.Entity<Endereco>().HasIndex(e => e.CEP);
        modelBuilder.Entity<PerfilConta>().HasKey(p => p.UsuarioId);
        modelBuilder.Entity<SegurancaConta>().HasKey(s => s.UsuarioId);
        modelBuilder.Entity<Favorito>().HasIndex(f => new { f.UsuarioId, f.ProdutoId }).IsUnique();
        modelBuilder.Entity<SessaoConta>().HasIndex(s => s.ChaveSessao).IsUnique();
        modelBuilder.Entity<TokenConta>().HasIndex(t => new { t.TokenHash, t.Finalidade }).IsUnique();
        modelBuilder.Entity<MetodoPagamentoSalvo>().HasIndex(m => m.UsuarioId);

        modelBuilder.Entity<PerfilConta>()
            .HasOne(p => p.Usuario)
            .WithOne(u => u.PerfilConta)
            .HasForeignKey<PerfilConta>(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SegurancaConta>()
            .HasOne(s => s.Usuario)
            .WithOne(u => u.SegurancaConta)
            .HasForeignKey<SegurancaConta>(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Favorito>()
            .HasOne(f => f.Usuario).WithMany().HasForeignKey(f => f.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Favorito>()
            .HasOne(f => f.Produto).WithMany().HasForeignKey(f => f.ProdutoId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MetodoPagamentoSalvo>()
            .HasOne(m => m.Usuario).WithMany().HasForeignKey(m => m.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SessaoConta>()
            .HasOne(s => s.Usuario).WithMany().HasForeignKey(s => s.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TokenConta>()
            .HasOne(t => t.Usuario).WithMany().HasForeignKey(t => t.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SolicitacaoSuporte>()
            .HasOne(s => s.Usuario).WithMany().HasForeignKey(s => s.UsuarioId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Carrinho>()
            .HasMany(c => c.Itens)
            .WithOne(i => i.Carrinho)
            .HasForeignKey(i => i.CarrinhoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Pedido>()
            .HasMany(p => p.Itens)
            .WithOne(i => i.Pedido)
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.EnderecoEntrega)
            .WithMany()
            .HasForeignKey(p => p.EnderecoEntregaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Usuario)
            .WithMany(u => u.Pedidos)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pagamento>()
            .HasOne(p => p.Pedido)
            .WithOne(p => p.Pagamento)
            .HasForeignKey<Pagamento>(p => p.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
