﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FfctestProject.Models;

[Table("ExpenseReportImages")]
public partial class ExpenseReportImage
{
    public int Id { get; set; }

    public int ExpenseReportId { get; set; }

    public string ImagePath { get; set; } = null!;

    public DateTime CreateDate { get; set; }
}
