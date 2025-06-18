﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FfctestProject.Models;

public partial class FfctestContext : DbContext
{
    public FfctestContext(DbContextOptions<FfctestContext> options)
        : base(options)
    {
    }

    //public virtual DbSet<ExpenseReportImage> ExpenseReportImages { get; set; }
    //public virtual DbSet<ExpenseApprovalHistory> ExpenseApprovalHistories { get; set; }
    //public virtual DbSet<ExpenseDefaultFinanceApprover> ExpenseDefaultFinanceApprovers { get; set; }
    public virtual DbSet<CRMLeadTransactionImage> CRMLeadTransactionImages { get; set; }
    //public virtual DbSet<User> Users { get; set; }
    //public virtual DbSet<CRMProduct> CRMProducts { get; set; }
    //public virtual DbSet<DlwCRMProductsLoad> DlwCRMProductsLoads { get; set; }
}

