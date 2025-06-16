using System;
using System.Collections.Generic;

namespace FfctestProject.Models;

public partial class ExpenseDefaultFinanceApprover
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CompanyId { get; set; }

    public DateTime CreateDate { get; set; }
}
