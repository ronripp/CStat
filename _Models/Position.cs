using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class Position
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        public int Title { get; set; }
        [Column("Person_id")]
        public int PersonId { get; set; }
        [Column("Event_id")]
        public int? EventId { get; set; }
        public int Status { get; set; }
        [Column("Start_Date", TypeName = "datetime2(0)")]
        public DateTime StartDate { get; set; }
        [Column("End_Date")]
        public DateTime? EndDate { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? Pay { get; set; }
        [Column("Pay_Terms")]
        public int PayTerms { get; set; }
        [Column("Responsibility_Link")]
        [StringLength(255)]
        public string ResponsibilityLink { get; set; }
        [StringLength(255)]
        public string Comments { get; set; }
        public long? Roles { get; set; }

        [ForeignKey(nameof(PersonId))]
        [InverseProperty("Position")]
        public virtual Person Person { get; set; }
    }
}
