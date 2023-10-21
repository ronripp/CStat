using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CStat.Models
{
    [Table("Incident", Schema = "ronripp_CStat")]
    public partial class Incident
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        public int Type { get; set; }
        [Column(TypeName = "datetime2(0)")]
        public DateTime Date { get; set; }
        [Column("Date_Reported", TypeName = "datetime2(0)")]
        public DateTime? DateReported { get; set; }
        [StringLength(255)]
        public string Description { get; set; }
        [Column("Person1_id")]
        public int? Person1Id { get; set; }
        [Column("Person2_id")]
        public int? Person2Id { get; set; }
        [Column("Persion3_id")]
        public int? Persion3Id { get; set; }
        [Column("Person4_id")]
        public int? Person4Id { get; set; }
        [Column("Person5_id")]
        public int? Person5Id { get; set; }
        [Column("Report_Link")]
        [StringLength(255)]
        public string ReportLink { get; set; }
        public long? Status { get; set; }

        [ForeignKey(nameof(Persion3Id))]
        [InverseProperty(nameof(Person.IncidentPersion3))]
        public virtual Person Persion3 { get; set; }
        [ForeignKey(nameof(Person1Id))]
        [InverseProperty(nameof(Person.IncidentPerson1))]
        public virtual Person Person1 { get; set; }
        [ForeignKey(nameof(Person2Id))]
        [InverseProperty(nameof(Person.IncidentPerson2))]
        public virtual Person Person2 { get; set; }
        [ForeignKey(nameof(Person4Id))]
        [InverseProperty(nameof(Person.IncidentPerson4))]
        public virtual Person Person4 { get; set; }
        [ForeignKey(nameof(Person5Id))]
        [InverseProperty(nameof(Person.IncidentPerson5))]
        public virtual Person Person5 { get; set; }
    }
}
