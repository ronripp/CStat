using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class Address
    {
        public Address()
        {
            Business = new HashSet<Business>();
            Church = new HashSet<Church>();
            Person = new HashSet<Person>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [StringLength(30)]
        public string Street { get; set; }
        [Required]
        [StringLength(30)]
        public string Town { get; set; }
        [Required]
        [StringLength(20)]
        public string State { get; set; }
        [Required]
        [StringLength(9)]
        public string ZipCode { get; set; }
        [StringLength(20)]
        public string Phone { get; set; }
        [StringLength(20)]
        public string Fax { get; set; }
        [Required]
        [StringLength(3)]
        public string Country { get; set; }
        [StringLength(60)]
        public string WebSite { get; set; }

        [InverseProperty("Address")]
        public virtual ICollection<Business> Business { get; set; }
        [InverseProperty("Address")]
        public virtual ICollection<Church> Church { get; set; }
        [InverseProperty("Address")]
        [NotMapped]
        public virtual ICollection<Person> Person { get; set; }
    }
}
