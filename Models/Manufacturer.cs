using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class Manufacturer
    {
        [StringLength(10)]
        public string Name { get; set; }
        [Column("Address_id")]
        public int? AddressId { get; set; }
        [Required]
        [Column("Contract_Link")]
        [StringLength(50)]
        public string ContractLink { get; set; }
    }
}
