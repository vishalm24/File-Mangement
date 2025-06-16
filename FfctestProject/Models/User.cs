using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FfctestProject.Models
{
    [Table("User")]
    public class User
    {
        public int UserId { get; set; }
        public string UserPassword { get; set; }
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string? CountryCode { get; set; }
        public string? ContactNo { get; set; }
        public string? Email { get; set; }
        public string Gender { get; set; }
        public DateTime Dob { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? StreetAddress { get; set; }
        public int? ZipCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string? UserImage { get; set; }
        public bool IsAdmin { get; set; }
        public bool? IsValidEmail { get; set; }
        public decimal? Rating { get; set; }
        public int? EmailOtp { get; set; }
        public string? TimeZone { get; set; }

        public int CountryId { get; set; }
        public int StateId { get; set; }
        public int CityId { get; set; }

        public bool IsDeleted { get; set; }
        public string? EmployeeNo { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? AccessWebLogin { get; set; }
        public int LocalityId { get; set; }
        public bool IsManager { get; set; }
    }
}
