using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CStat.Models
{
    [Table("Account", Schema = "ronripp_CStat")]
    public partial class Account
    {
        public Account()
        {
            Transaction = new HashSet<Transaction>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        public int Type { get; set; }
        [Required]
        [Column("Account_Num")]
        [StringLength(50)]
        public string AccountNum { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(255)]
        public string Terms { get; set; }
        [Column("Business_id")]
        public int BusinessId { get; set; }
        [Column("Contract_Link")]
        [StringLength(255)]
        public string ContractLink { get; set; }

        [ForeignKey(nameof(BusinessId))]
        [InverseProperty("Account")]
        public virtual Business Business { get; set; }
        [InverseProperty("CcaAccount")]
        public virtual ICollection<Transaction> Transaction { get; set; }
    }
}
