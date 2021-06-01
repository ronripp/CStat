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
            InverseParentTask = new HashSet<Task>();
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
        [Column("Person_id")]
        public int? PersonId { get; set; }
        [Column("Due_Date", TypeName = "datetime2(0)")]
        public DateTime? DueDate { get; set; }
        [Column("Creation_Date", TypeName = "datetime2(0)")]
        public DateTime CreationDate { get; set; }
        [Column("Start_Date", TypeName = "datetime2(0)")]
        public DateTime? StartDate { get; set; }
        [Column("Actual_Done_Date", TypeName = "datetime2(0)")]
        public DateTime? ActualDoneDate { get; set; }
        [Column("Church_id")]
        public int? ChurchId { get; set; }
        [Column("Event_id")]
        public int? EventId { get; set; }
        [Column("Plan_Link")]
        [StringLength(255)]
        public string PlanLink { get; set; }
        [Required]
        [Column("Required_Skills")]
        [StringLength(255)]
        public string RequiredSkills { get; set; }
        [Column("Committed_Cost", TypeName = "money")]
        public decimal? CommittedCost { get; set; }
        [Column("Committed_Man_Hours")]
        public double? CommittedManHours { get; set; }
        [Column("Estimated_Done_Date")]
        public DateTime? EstimatedDoneDate { get; set; }
        [Column("Estimated_Man_Hours")]
        public double? EstimatedManHours { get; set; }
        public long? Roles { get; set; }
        public int Status { get; set; }
        [Column("Total_Cost", TypeName = "money")]
        public decimal? TotalCost { get; set; }
        [Column("Worker1_id")]
        public int? Worker1Id { get; set; }
        [Column("Worker2_id")]
        public int? Worker2Id { get; set; }
        [Column("Worker3_id")]
        public int? Worker3Id { get; set; }
        [Column("ParentTask_id")]
        public int? ParentTaskId { get; set; }

        [ForeignKey(nameof(Blocking1Id))]
        [InverseProperty(nameof(Task.InverseBlocking1))]
        public virtual Task Blocking1 { get; set; }
        [ForeignKey(nameof(Blocking2Id))]
        [InverseProperty(nameof(Task.InverseBlocking2))]
        public virtual Task Blocking2 { get; set; }
        [ForeignKey(nameof(ChurchId))]
        [InverseProperty("Task")]
        public virtual Church Church { get; set; }
        [ForeignKey(nameof(EventId))]
        [InverseProperty("Task")]
        public virtual Event Event { get; set; }
        [ForeignKey(nameof(ParentTaskId))]
        [InverseProperty(nameof(Task.InverseParentTask))]
        public virtual Task ParentTask { get; set; }
        [ForeignKey(nameof(PersonId))]
        [InverseProperty("TaskPerson")]
        public virtual Person Person { get; set; }
        [ForeignKey(nameof(Worker1Id))]
        [InverseProperty("TaskWorker1")]
        public virtual Person Worker1 { get; set; }
        [ForeignKey(nameof(Worker2Id))]
        [InverseProperty("TaskWorker2")]
        public virtual Person Worker2 { get; set; }
        [ForeignKey(nameof(Worker3Id))]
        [InverseProperty("TaskWorker3")]
        public virtual Person Worker3 { get; set; }
        [InverseProperty(nameof(Task.Blocking1))]
        public virtual ICollection<Task> InverseBlocking1 { get; set; }
        [InverseProperty(nameof(Task.Blocking2))]
        public virtual ICollection<Task> InverseBlocking2 { get; set; }
        [InverseProperty(nameof(Task.ParentTask))]
        public virtual ICollection<Task> InverseParentTask { get; set; }
    }
}
