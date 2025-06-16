using System;
using System.Collections.Generic;

namespace FfctestProject.Models;

public partial class ExpenseApprovalHistory
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ExpenseReportId { get; set; }

    public int StatusId { get; set; }

    public int ApprovalOrderNumber { get; set; }

    public DateTime CreateDate { get; set; }

    public decimal ApprovedAmount { get; set; }
}
