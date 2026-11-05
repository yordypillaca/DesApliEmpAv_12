using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TallerApi.Models;

public partial class TallerContext : DbContext
{
    public TallerContext(DbContextOptions<TallerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Orden> Ordenes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Clientes_pkey");

            entity.HasIndex(e => e.Documento, "UQ_Clientes_Documento").IsUnique();

            entity.Property(e => e.Correo).HasMaxLength(120);
            entity.Property(e => e.Documento).HasMaxLength(15);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<Orden>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Ordenes_pkey");

            entity.HasIndex(e => e.Numero, "UQ_Ordenes_Numero").IsUnique();

            entity.Property(e => e.CostoEstimado).HasPrecision(10, 2);
            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasMaxLength(20);
            entity.Property(e => e.Numero).HasMaxLength(20);
            entity.Property(e => e.Placa).HasMaxLength(10);

            entity.HasOne(d => d.Cliente).WithMany(p => p.Ordenes)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ordenes_Clientes");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
