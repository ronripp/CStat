using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class Task
    {
        public Task()
        {
            InverseBlocking1 = new HashSet<Task>();
            InverseBlocking2 = new HashSet<Task>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Description { get; set; }
        [Column("priority")]
        public int Priority { get; set; }
        [Column("Blocking1_id")]
        public int? Blocking1Id { get; set; }
        [Column("Blocking2_id")]
        public int? Blocking2Id { get; set; }
        public int Type { get; set; }
        [Column("Task_Status")]
        public int TaskStatus { get; set; }
        [Column("Person_id")]
        public int? PersonId { get; set; }
        [Column("Position_Title1")]
        public int? PositionTitle1 { get; set; }
        [Column("Position_Tile2")]
        public int? PositionTile2 { get; set; }
        [Column("Position_Title3")]
        public int? PositionTitle3 { get; set; }
        [Column("Due_Date", TypeName = "datetime2(0)")]
        public DateTime? DueDate { get; set; }
        [Column("Creation_Date", TypeName = "datetime2(0)")]
        public DateTime CreationDate { get; set; }
        [Column("Start_Date", TypeName = "datetime2(0)")]
        public DateTime? StartDate { get; set; }
        [Column("Completion_Date", TypeName = "datetime2(0)")]
        public DateTime? CompletionDate { get; set; }
        [Column("Church_id")]
        public int? ChurchId { get; set; }
        [Column("Plan_Link")]
        [StringLength(255)]
        public string PlanLink { get; set; }
        [Required]
        [Column("Required_Skills")]
        [StringLength(255)]
        public string RequiredSkills { get; set; }

        [ForeignKey(nameof(Blocking1Id))]
        [InverseProperty(nameof(Task.InverseBlocking1))]
        public virtual Task Blocking1 { get; set; }
        [ForeignKey(nameof(Blocking2Id))]
        [InverseProperty(nameof(Task.InverseBlocking2))]
        public virtual Task Blocking2 { get; set; }
        [ForeignKey(nameof(ChurchId))]
        [InverseProperty("Task")]
        public virtual Church Church { get; set; }
        [ForeignKey(nameof(PersonId))]
        [InverseProperty("Task")]
        public virtual Person Person { get; set; }
        [InverseProperty(nameof(Task.Blocking1))]
        public virtual ICollection<Task> InverseBlocking1 { get; set; }
        [InverseProperty(nameof(Task.Blocking2))]
        public virtual ICollection<Task> InverseBlocking2 { get; set; }
    }
}
