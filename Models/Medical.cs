using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class Medical
    {
        public Medical()
        {
            Attendance = new HashSet<Attendance>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("Person_id")]
        public int PersonId { get; set; }
        [Column("Event_id")]
        public int? EventId { get; set; }
        [Required]
        [Column("Form_Link")]
        [StringLength(255)]
        public string FormLink { get; set; }

        [ForeignKey(nameof(EventId))]
        [InverseProperty("Medical")]
        public virtual Event Event { get; set; }
        [ForeignKey(nameof(PersonId))]
        [InverseProperty("Medical")]
        public virtual Person Person { get; set; }
        [InverseProperty("Medical")]
        public virtual ICollection<Attendance> Attendance { get; set; }
    }
}
