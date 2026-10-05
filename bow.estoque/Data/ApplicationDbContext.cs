using bow.estoque.Models;
using Microsoft.EntityFrameworkCore;

namespace bow.estoque.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
    public DbSet<Venda> Vendas { get; set; }
    public DbSet<ItemVenda> ItensVenda { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasIndex(u => u.NomeUsuario).IsUnique();
        modelBuilder.Entity<Produto>().HasIndex(p => p.SKU).IsUnique();
        modelBuilder.Entity<Fornecedor>().HasIndex(f => f.CNPJ).IsUnique();
        modelBuilder.Entity<Cliente>().HasIndex(c => c.CPF).IsUnique();

        modelBuilder.Entity<Produto>().HasOne(p => p.Categoria).WithMany(c => c.Produtos).HasForeignKey(p => p.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Produto>().HasOne(p => p.Fornecedor).WithMany(f => f.Produtos).HasForeignKey(p => p.FornecedorId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MovimentacaoEstoque>().HasOne(m => m.Produto).WithMany(p => p.Movimentacoes).HasForeignKey(m => m.ProdutoId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MovimentacaoEstoque>().HasOne(m => m.Usuario).WithMany().HasForeignKey(m => m.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Venda>().HasOne(v => v.Cliente).WithMany(c => c.Vendas).HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Venda>().HasOne(v => v.Usuario).WithMany().HasForeignKey(v => v.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ItemVenda>().HasOne(i => i.Produto).WithMany().HasForeignKey(i => i.ProdutoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ItemVenda>().HasOne(i => i.Venda).WithMany(v => v.Itens).HasForeignKey(i => i.VendaId).OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
