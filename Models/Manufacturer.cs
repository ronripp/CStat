﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CStat.Models
{
    [Table("Manufacturer", Schema = "ronripp_CStat")]
    public partial class Manufacturer
    {
        public Manufacturer()
        {
            Item = new HashSet<Item>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [StringLength(10)]
        public string Name { get; set; }
        [Column("Address_id")]
        public int? AddressId { get; set; }
        [Required]
        [Column("Contract_Link")]
        [StringLength(50)]
        public string ContractLink { get; set; }

        [InverseProperty("MfgNavigation")]
        public virtual ICollection<Item> Item { get; set; }
    }
}
