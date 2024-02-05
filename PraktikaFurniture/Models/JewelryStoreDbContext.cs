using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PraktikaFurniture.Models;

public partial class JewelryStoreDbContext : DbContext
{
    public static JewelryStoreDbContext dbContext = new JewelryStoreDbContext();
    public JewelryStoreDbContext()
    {
    }

    public JewelryStoreDbContext(DbContextOptions<JewelryStoreDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Jewelry> Jewelries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=JewelryStoreDB;User Id=Adelya;Password=Adelya27; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Jewelry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Jewelry__3213E83FF483F352");

            entity.ToTable("Jewelry");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Gemstone)
                .HasMaxLength(30)
                .HasColumnName("gemstone");
            entity.Property(e => e.Metal)
                .HasMaxLength(20)
                .HasColumnName("metal");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
