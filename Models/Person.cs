using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace CStat.Models
{
    public partial class Person
    {
        public Person()
        {
            Attendance = new HashSet<Attendance>();
            Business = new HashSet<Business>();
            ChurchAlternate1 = new HashSet<Church>();
            ChurchAlternate2 = new HashSet<Church>();
            ChurchAlternate3 = new HashSet<Church>();
            ChurchElder1 = new HashSet<Church>();
            ChurchElder2 = new HashSet<Church>();
            ChurchElder3 = new HashSet<Church>();
            ChurchElder4 = new HashSet<Church>();
            ChurchElder5 = new HashSet<Church>();
            ChurchSeniorMinister = new HashSet<Church>();
            ChurchTrustee1 = new HashSet<Church>();
            ChurchTrustee2 = new HashSet<Church>();
            ChurchTrustee3 = new HashSet<Church>();
            ChurchYouthMinister = new HashSet<Church>();
            IncidentPersion3 = new HashSet<Incident>();
            IncidentPerson1 = new HashSet<Incident>();
            IncidentPerson2 = new HashSet<Incident>();
            IncidentPerson4 = new HashSet<Incident>();
            IncidentPerson5 = new HashSet<Incident>();
            InventoryItem = new HashSet<InventoryItem>();
            InversePg1Person = new HashSet<Person>();
            InversePg2Person = new HashSet<Person>();
            Medical = new HashSet<Medical>();
            Operations = new HashSet<Operations>();
            Position = new HashSet<Position>();
            Registration = new HashSet<Registration>();
            Task = new HashSet<Task>();
            Transaction = new HashSet<Transaction>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [StringLength(30)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(30)]
        public string LastName { get; set; }
        [StringLength(30)]
        public string Alias { get; set; }
        [Column("DOB", TypeName = "date")]
        public DateTime? Dob { get; set; }
        public byte? Gender { get; set; }
        public long? Status { get; set; }
        [Column("SSNum")]
        [StringLength(10)]
        public string Ssnum { get; set; }
        [Column("Address_id")]
        public int? AddressId { get; set; }
        [Column("PG1_Person_id")]
        public int? Pg1PersonId { get; set; }
        [Column("PG2_Person_id")]
        public int? Pg2PersonId { get; set; }
        [Column("Church_id")]
        public int? ChurchId { get; set; }
        public long SkillSets { get; set; }
        [StringLength(20)]
        public string CellPhone { get; set; }
        [Column("EMail")]
        [StringLength(50)]
        public string Email { get; set; }
        [StringLength(50)]
        public string ContactPref { get; set; }
        [StringLength(255)]
        public string Notes { get; set; }
        public long? Roles { get; set; }

        [ForeignKey(nameof(AddressId))]
        [InverseProperty("Person")]
        public virtual Address Address { get; set; }
        [ForeignKey(nameof(ChurchId))]
        [InverseProperty("Person")]
        public virtual Church Church { get; set; }
        [ForeignKey(nameof(Pg1PersonId))]
        [InverseProperty(nameof(Person.InversePg1Person))]
        public virtual Person Pg1Person { get; set; }
        [ForeignKey(nameof(Pg2PersonId))]
        [InverseProperty(nameof(Person.InversePg2Person))]
        public virtual Person Pg2Person { get; set; }
        [InverseProperty("Person")]
        public virtual ICollection<Attendance> Attendance { get; set; }
        [InverseProperty("Poc")]
        public virtual ICollection<Business> Business { get; set; }
        [InverseProperty("Alternate1")]
        public virtual ICollection<Church> ChurchAlternate1 { get; set; }
        [InverseProperty("Alternate2")]
        public virtual ICollection<Church> ChurchAlternate2 { get; set; }
        [InverseProperty("Alternate3")]
        public virtual ICollection<Church> ChurchAlternate3 { get; set; }
        [InverseProperty("Elder1")]
        public virtual ICollection<Church> ChurchElder1 { get; set; }
        [InverseProperty("Elder2")]
        public virtual ICollection<Church> ChurchElder2 { get; set; }
        [InverseProperty("Elder3")]
        public virtual ICollection<Church> ChurchElder3 { get; set; }
        [InverseProperty("Elder4")]
        public virtual ICollection<Church> ChurchElder4 { get; set; }
        [InverseProperty("Elder5")]
        public virtual ICollection<Church> ChurchElder5 { get; set; }
        [InverseProperty("SeniorMinister")]
        public virtual ICollection<Church> ChurchSeniorMinister { get; set; }
        [InverseProperty("Trustee1")]
        public virtual ICollection<Church> ChurchTrustee1 { get; set; }
        [InverseProperty("Trustee2")]
        public virtual ICollection<Church> ChurchTrustee2 { get; set; }
        [InverseProperty("Trustee3")]
        public virtual ICollection<Church> ChurchTrustee3 { get; set; }
        [InverseProperty("YouthMinister")]
        public virtual ICollection<Church> ChurchYouthMinister { get; set; }
        [InverseProperty(nameof(Incident.Persion3))]
        public virtual ICollection<Incident> IncidentPersion3 { get; set; }
        [InverseProperty(nameof(Incident.Person1))]
        public virtual ICollection<Incident> IncidentPerson1 { get; set; }
        [InverseProperty(nameof(Incident.Person2))]
        public virtual ICollection<Incident> IncidentPerson2 { get; set; }
        [InverseProperty(nameof(Incident.Person4))]
        public virtual ICollection<Incident> IncidentPerson4 { get; set; }
        [InverseProperty(nameof(Incident.Person5))]
        public virtual ICollection<Incident> IncidentPerson5 { get; set; }
        [InverseProperty("Person")]
        public virtual ICollection<InventoryItem> InventoryItem { get; set; }
        [InverseProperty(nameof(Person.Pg1Person))]
        public virtual ICollection<Person> InversePg1Person { get; set; }
        [InverseProperty(nameof(Person.Pg2Person))]
        public virtual ICollection<Person> InversePg2Person { get; set; }
        [InverseProperty("Person")]
        public virtual ICollection<Medical> Medical { get; set; }
        [InverseProperty("Person")]
        public virtual ICollection<Operations> Operations { get; set; }
        [InverseProperty("Person")]
        public virtual ICollection<Position> Position { get; set; }
        [InverseProperty("Person")]
        public virtual ICollection<Registration> Registration { get; set; }
        [InverseProperty("Person")]
        public virtual ICollection<Task> Task { get; set; }
        [InverseProperty("CcaPerson")]
        public virtual ICollection<Transaction> Transaction { get; set; }

        public static implicit operator Task<object>(Person v)
        {
            throw new NotImplementedException();
        }
    }
}
