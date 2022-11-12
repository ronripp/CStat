using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CStat.Models
{
    [Table("TransactionItems", Schema = "ronripp_CStat")]
    public partial class TransactionItems
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [Column("Transaction_id")]
        public int TransactionId { get; set; }
        [Column("item_id")]
        public int ItemId { get; set; }
        [Column(TypeName = "money")]
        public decimal Cost { get; set; }

        [ForeignKey(nameof(Id))]
        [InverseProperty(nameof(Transaction.TransactionItems))]
        public virtual Transaction Id1 { get; set; }
        [ForeignKey(nameof(Id))]
        [InverseProperty(nameof(Item.TransactionItems))]
        public virtual Item IdNavigation { get; set; }
    }
}
