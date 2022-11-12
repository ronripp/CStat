using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CStat.Models
{
    [Table("AspNetRoleClaims", Schema = "ronripp_CStat")]
    public partial class AspNetRoleClaims
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string RoleId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }

        [ForeignKey(nameof(RoleId))]
        [InverseProperty(nameof(AspNetRoles.AspNetRoleClaims))]
        public virtual AspNetRoles Role { get; set; }
    }
}
