using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfctestProject.Models
{
    public partial class CRMLeadTransactionImage
    {
        public int Id { get; set; }
        public int LeadTransactionId { get; set; }
        public string ImagePath { get; set; } = null;
        public DateTime CreateDate { get; set; }
    }
}
