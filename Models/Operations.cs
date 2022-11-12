using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CStat.Models
{
    [Table("Operations", Schema = "ronripp_CStat")]
    public partial class Operations
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        public int Type { get; set; }
        [Column("time", TypeName = "datetime2(0)")]
        public DateTime Time { get; set; }
        [Column("result")]
        public int? Result { get; set; }
        [Column("Person_id")]
        public int? PersonId { get; set; }
        [Column("Church_id")]
        public int? ChurchId { get; set; }
        [Column("Business_id")]
        public int? BusinessId { get; set; }
        [Required]
        [Column("Record_Link")]
        [StringLength(255)]
        public string RecordLink { get; set; }

        [ForeignKey(nameof(BusinessId))]
        [InverseProperty("Operations")]
        public virtual Business Business { get; set; }
        [ForeignKey(nameof(ChurchId))]
        [InverseProperty("Operations")]
        public virtual Church Church { get; set; }
        [ForeignKey(nameof(PersonId))]
        [InverseProperty("Operations")]
        public virtual Person Person { get; set; }
    }
}
