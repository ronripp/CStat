using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CStat.Models
{
    [Table("Event", Schema = "ronripp_CStat")]
    public partial class Event
    {
        public Event()
        {
            Attendance = new HashSet<Attendance>();
            Medical = new HashSet<Medical>();
            Registration = new HashSet<Registration>();
            Task = new HashSet<Task>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [Column("Start_Time", TypeName = "datetime2(0)")]
        public DateTime StartTime { get; set; }
        [Column("End_Time", TypeName = "datetime2(0)")]
        public DateTime EndTime { get; set; }
        public int Type { get; set; }
        [StringLength(255)]
        public string Description { get; set; }
        [Column("Church_id")]
        public int? ChurchId { get; set; }
        [Column("Cost_Child", TypeName = "decimal(10, 2)")]
        public decimal? CostChild { get; set; }
        [Column("Cost_Adult", TypeName = "decimal(10, 2)")]
        public decimal? CostAdult { get; set; }
        [Column("Cost_Family", TypeName = "decimal(10, 2)")]
        public decimal? CostFamily { get; set; }
        [Column("Cost_Cabin", TypeName = "decimal(10, 2)")]
        public decimal? CostCabin { get; set; }
        [Column("Cost_Lodge", TypeName = "decimal(10, 2)")]
        public decimal? CostLodge { get; set; }
        [Column("Cost_Tent", TypeName = "decimal(10, 2)")]
        public decimal? CostTent { get; set; }
        [Column("Contract_Link")]
        [StringLength(255)]
        public string ContractLink { get; set; }
        [Column("staff")]
        public int? Staff { get; set; }

        [ForeignKey(nameof(ChurchId))]
        [InverseProperty("Event")]
        public virtual Church Church { get; set; }
        [InverseProperty("Event")]
        public virtual ICollection<Attendance> Attendance { get; set; }
        [InverseProperty("Event")]
        public virtual ICollection<Medical> Medical { get; set; }
        [InverseProperty("Event")]
        public virtual ICollection<Registration> Registration { get; set; }
        [InverseProperty("Event")]
        public virtual ICollection<Task> Task { get; set; }
    }
}
