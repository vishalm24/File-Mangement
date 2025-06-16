using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfctestProject.Models
{
    [Table("DlwCRMProductsLoad")]
    public class DlwCRMProductsLoad
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? Name { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string SKUCode { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public int AvailableQuantity { get; set; }
        public bool IsProductGroup { get; set; }
        public int TotalQuantity { get; set; }
        public bool IsChildProduct { get; set; }
        public bool IsSearchable { get; set; }
        public decimal ProductWeight { get; set; }
        public decimal ProductLength { get; set; }
        public decimal ProductWidth { get; set; }
        public decimal ProductHeight { get; set; }
        public int NoOfPieces { get; set; }
        public bool HasChildProduct { get; set; }
        public string ProductSize { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public int? FileInfoId { get; set; }
    }
}
