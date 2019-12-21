using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class Account
    {
        public Account()
        {
            Transaction = new HashSet<Transaction>();
        }

        [Key]
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
