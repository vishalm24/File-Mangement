using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FfctestProject.Models;

public partial class FfctestContext : DbContext
{
    public FfctestContext(DbContextOptions<FfctestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ExpenseReportImage> ExpenseReportImages { get; set; }
    public virtual DbSet<ExpenseApprovalHistory> ExpenseApprovalHistories { get; set; }
    public virtual DbSet<ExpenseDefaultFinanceApprover> ExpenseDefaultFinanceApprovers { get; set; }
}
