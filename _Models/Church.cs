using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CStat.Models
{
    [Table("Church", Schema = "ronripp_CStat")]
    public partial class Church
    {
        public Church()
        {
            Event = new HashSet<Event>();
            Operations = new HashSet<Operations>();
            Person = new HashSet<Person>();
            Task = new HashSet<Task>();
            Transaction = new HashSet<Transaction>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Column("Address_id")]
        public int? AddressId { get; set; }
        [Required]
        [StringLength(50)]
        public string Affiliation { get; set; }
        [Column("Membership_Status")]
        public int MembershipStatus { get; set; }
        [Column("Status_Details")]
        [StringLength(255)]
        public string StatusDetails { get; set; }
        [Column("Senior_Minister_id")]
        public int? SeniorMinisterId { get; set; }
        [Column("Youth_Minister_id")]
        public int? YouthMinisterId { get; set; }
        [Column("Trustee1_id")]
        public int? Trustee1Id { get; set; }
        [Column("Trustee2_id")]
        public int? Trustee2Id { get; set; }
        [Column("Trustee3_id")]
        public int? Trustee3Id { get; set; }
        [Column("Alternate1_id")]
        public int? Alternate1Id { get; set; }
        [Column("Alternate2_id")]
        public int? Alternate2Id { get; set; }
        [Column("Alternate3_id")]
        public int? Alternate3Id { get; set; }
        [Column("Elder1_id")]
        public int? Elder1Id { get; set; }
        [Column("Elder2_id")]
        public int? Elder2Id { get; set; }
        [Column("Elder3_id")]
        public int? Elder3Id { get; set; }
        [Column("Elder4_id")]
        public int? Elder4Id { get; set; }
        [Column("Elder5_id")]
        public int? Elder5Id { get; set; }
        [Column("EMail")]
        [StringLength(100)]
        public string Email { get; set; }

        [ForeignKey(nameof(AddressId))]
        [InverseProperty("Church")]
        public virtual Address Address { get; set; }
        [ForeignKey(nameof(Alternate1Id))]
        [InverseProperty("ChurchAlternate1")]
        public virtual Person Alternate1 { get; set; }
        [ForeignKey(nameof(Alternate2Id))]
        [InverseProperty("ChurchAlternate2")]
        public virtual Person Alternate2 { get; set; }
        [ForeignKey(nameof(Alternate3Id))]
        [InverseProperty("ChurchAlternate3")]
        public virtual Person Alternate3 { get; set; }
        [ForeignKey(nameof(Elder1Id))]
        [InverseProperty("ChurchElder1")]
        public virtual Person Elder1 { get; set; }
        [ForeignKey(nameof(Elder2Id))]
        [InverseProperty("ChurchElder2")]
        public virtual Person Elder2 { get; set; }
        [ForeignKey(nameof(Elder3Id))]
        [InverseProperty("ChurchElder3")]
        public virtual Person Elder3 { get; set; }
        [ForeignKey(nameof(Elder4Id))]
        [InverseProperty("ChurchElder4")]
        public virtual Person Elder4 { get; set; }
        [ForeignKey(nameof(Elder5Id))]
        [InverseProperty("ChurchElder5")]
        public virtual Person Elder5 { get; set; }
        [ForeignKey(nameof(SeniorMinisterId))]
        [InverseProperty("ChurchSeniorMinister")]
        public virtual Person SeniorMinister { get; set; }
        [ForeignKey(nameof(Trustee1Id))]
        [InverseProperty("ChurchTrustee1")]
        public virtual Person Trustee1 { get; set; }
        [ForeignKey(nameof(Trustee2Id))]
        [InverseProperty("ChurchTrustee2")]
        public virtual Person Trustee2 { get; set; }
        [ForeignKey(nameof(Trustee3Id))]
        [InverseProperty("ChurchTrustee3")]
        public virtual Person Trustee3 { get; set; }
        [ForeignKey(nameof(YouthMinisterId))]
        [InverseProperty("ChurchYouthMinister")]
        public virtual Person YouthMinister { get; set; }
        [InverseProperty("Church")]
        public virtual ICollection<Event> Event { get; set; }
        [InverseProperty("Church")]
        public virtual ICollection<Operations> Operations { get; set; }
        [InverseProperty("Church")]
        public virtual ICollection<Person> Person { get; set; }
        [InverseProperty("Church")]
        public virtual ICollection<Task> Task { get; set; }
        [InverseProperty("Church")]
        public virtual ICollection<Transaction> Transaction { get; set; }
    }
}
