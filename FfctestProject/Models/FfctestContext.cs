using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FfctestProject.Models;

public partial class FfctestContext : DbContext
{
    public FfctestContext()
    {
    }

    public FfctestContext(DbContextOptions<FfctestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ExpenseApprovalHistory> ExpenseApprovalHistories { get; set; }

    public virtual DbSet<ExpenseDefaultFinanceApprover> ExpenseDefaultFinanceApprovers { get; set; }

    public virtual DbSet<ExpenseReportImage> ExpenseReportImages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=FFCTest;Trusted_Connection=True; TrustServerCertificate = True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExpenseApprovalHistory>(entity =>
        {
            entity.ToTable("ExpenseApprovalHistory");

            entity.Property(e => e.ApprovedAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<ExpenseDefaultFinanceApprover>(entity =>
        {
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<ExpenseReportImage>(entity =>
        {
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
