﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CStat.Models
{
    [Table("Business", Schema = "ronripp_CStat")]
    public partial class Business
    {
        public Business()
        {
            Account = new HashSet<Account>();
            Item = new HashSet<Item>();
            Operations = new HashSet<Operations>();
            Transaction = new HashSet<Transaction>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        [Column("Address_id")]
        public int? AddressId { get; set; }
        public int? Type { get; set; }
        [StringLength(255)]
        public string Terms { get; set; }
        [StringLength(255)]
        public string Fees { get; set; }
        public int? Status { get; set; }
        [Column("Status_Details")]
        [StringLength(255)]
        public string StatusDetails { get; set; }
        [Column("POC_id")]
        public int? PocId { get; set; }
        [Column("Contract_Link")]
        [StringLength(255)]
        public string ContractLink { get; set; }
        [Column("User_Link")]
        [StringLength(255)]
        public string UserLink { get; set; }
        [Column("API_Link")]
        [StringLength(255)]
        public string ApiLink { get; set; }
        [Column("Acct_id")]
        [StringLength(50)]
        public string AcctId { get; set; }
        [Column("User_Name")]
        [StringLength(30)]
        public string UserName { get; set; }
        [StringLength(30)]
        public string Password { get; set; }

        [ForeignKey(nameof(AddressId))]
        [InverseProperty("Business")]
        public virtual Address Address { get; set; }
        [ForeignKey(nameof(PocId))]
        [InverseProperty(nameof(Person.Business))]
        public virtual Person Poc { get; set; }
        [InverseProperty("Business")]
        public virtual ICollection<Account> Account { get; set; }
        [InverseProperty("Mfg")]
        public virtual ICollection<Item> Item { get; set; }
        [InverseProperty("Business")]
        public virtual ICollection<Operations> Operations { get; set; }
        [InverseProperty("Business")]
        public virtual ICollection<Transaction> Transaction { get; set; }
    }
}
