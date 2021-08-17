using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class Registration
    {
        public Registration()
        {
            Attendance = new HashSet<Attendance>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [Column("Event_id")]
        public int? EventId { get; set; }
        [Column("Person_id")]
        public int? PersonId { get; set; }
        [Required]
        [Column("Form_Link")]
        [StringLength(255)]
        public string FormLink { get; set; }
        [Column("Current_Grade")]
        public int? CurrentGrade { get; set; }
        [Column("T_Shirt_Size")]
        [StringLength(10)]
        public string TShirtSize { get; set; }

        [ForeignKey(nameof(EventId))]
        [InverseProperty("Registration")]
        public virtual Event Event { get; set; }
        [ForeignKey(nameof(PersonId))]
        [InverseProperty("Registration")]
        public virtual Person Person { get; set; }
        [InverseProperty("Registration")]
        public virtual ICollection<Attendance> Attendance { get; set; }
    }
}
