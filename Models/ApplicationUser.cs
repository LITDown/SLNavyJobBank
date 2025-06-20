using Microsoft.AspNetCore.Identity;
using System;

namespace SLNavyJobBank.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? FullName { get; set; }
        public string? NIC { get; set; }
        public string? NavyId { get; set; }
        public string? OfficialNo { get; set; }
        public string? ServiceType { get; set; }
        public string? OfficerSailor { get; set; }
        public string? Rank { get; set; }
        public string? Initials { get; set; }
        public string? PermanentAddress { get; set; }
        public string? Branch { get; set; }
        public string? CivilStatus { get; set; }
        public DateTime? DateOfMarriage { get; set; }
        public string? Establishment { get; set; }
        public DateTime? DOB { get; set; }
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; } = true;


    }
}
