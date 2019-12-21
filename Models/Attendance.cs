using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class Attendance
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("Person_id")]
        public int PersonId { get; set; }
        [Column("Role_Type")]
        public int RoleType { get; set; }
        [Column("Start_Time", TypeName = "datetime2(0)")]
        public DateTime? StartTime { get; set; }
        [Column("End_Time", TypeName = "datetime2(0)")]
        public DateTime? EndTime { get; set; }
        [Column("Event_id")]
        public int? EventId { get; set; }
        [Column("Registration_id")]
        public int? RegistrationId { get; set; }
        [Column("Medical_id")]
        public int? MedicalId { get; set; }
        [Column("Transaction_id")]
        public int? TransactionId { get; set; }
        [Column("Detail_Note")]
        [StringLength(255)]
        public string DetailNote { get; set; }

        [ForeignKey(nameof(EventId))]
        [InverseProperty("Attendance")]
        public virtual Event Event { get; set; }
        [ForeignKey(nameof(MedicalId))]
        [InverseProperty("Attendance")]
        public virtual Medical Medical { get; set; }
        [ForeignKey(nameof(PersonId))]
        [InverseProperty("Attendance")]
        public virtual Person Person { get; set; }
        [ForeignKey(nameof(RegistrationId))]
        [InverseProperty("Attendance")]
        public virtual Registration Registration { get; set; }
        [ForeignKey(nameof(TransactionId))]
        [InverseProperty("Attendance")]
        public virtual Transaction Transaction { get; set; }
    }
}
