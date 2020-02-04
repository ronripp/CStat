using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class TransactionItems
    {
        [Column("Transaction_id")]
        public int TransactionId { get; set; }
        [Column("item_id")]
        public int ItemId { get; set; }
        [Column(TypeName = "money")]
        public decimal Cost { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey(nameof(Id))]
        [InverseProperty(nameof(Transaction.TransactionItems))]
        public virtual Transaction Id1 { get; set; }
        [ForeignKey(nameof(Id))]
        [InverseProperty(nameof(Item.TransactionItems))]
        public virtual Item IdNavigation { get; set; }
    }
}
