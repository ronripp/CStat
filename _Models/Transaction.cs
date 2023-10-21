using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace CStat.Models
{
    [Table("Transaction", Schema = "ronripp_CStat")]
    public partial class Transaction
    {
        public Transaction()
        {
            Attendance = new HashSet<Attendance>();
            InventoryItemBuy1 = new HashSet<InventoryItem>();
            InventoryItemBuy2 = new HashSet<InventoryItem>();
            InventoryItemBuy3 = new HashSet<InventoryItem>();
            InventoryItemOrder = new HashSet<InventoryItem>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [Column("Income_Type")]
        public int? IncomeType { get; set; }
        [Column("Expense_Type")]
        public int? ExpenseType { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? Amount { get; set; }
        [Column("CCA_Person_id")]
        public int? CcaPersonId { get; set; }
        [Column("Business_id")]
        public int? BusinessId { get; set; }
        [Column("Church_id")]
        public int? ChurchId { get; set; }
        [Column(TypeName = "datetime2(0)")]
        public DateTime Date { get; set; }
        [Column("CCA_Account_id")]
        public int? CcaAccountId { get; set; }
        [Column("payment_type")]
        public int? PaymentType { get; set; }
        [Column("memo")]
        [StringLength(50)]
        public string Memo { get; set; }
        [Column("invoice_id")]
        [StringLength(10)]
        public string InvoiceId { get; set; }
        [Column("payment_number")]
        [StringLength(30)]
        public string PaymentNumber { get; set; }
        [Column("link")]
        [StringLength(700)]
        public string Link { get; set; }

        [ForeignKey(nameof(BusinessId))]
        [InverseProperty("Transaction")]
        public virtual Business Business { get; set; }
        [ForeignKey(nameof(CcaAccountId))]
        [InverseProperty(nameof(Account.Transaction))]
        public virtual Account CcaAccount { get; set; }
        [ForeignKey(nameof(CcaPersonId))]
        [InverseProperty(nameof(Person.Transaction))]
        public virtual Person CcaPerson { get; set; }
        [ForeignKey(nameof(ChurchId))]
        [InverseProperty("Transaction")]
        public virtual Church Church { get; set; }
        [InverseProperty("Id1")]
        public virtual TransactionItems TransactionItems { get; set; }
        [InverseProperty("Transaction")]
        public virtual ICollection<Attendance> Attendance { get; set; }
        [InverseProperty(nameof(InventoryItem.Buy1))]
        public virtual ICollection<InventoryItem> InventoryItemBuy1 { get; set; }
        [InverseProperty(nameof(InventoryItem.Buy2))]
        public virtual ICollection<InventoryItem> InventoryItemBuy2 { get; set; }
        [InverseProperty(nameof(InventoryItem.Buy3))]
        public virtual ICollection<InventoryItem> InventoryItemBuy3 { get; set; }
        [InverseProperty(nameof(InventoryItem.Order))]
        public virtual ICollection<InventoryItem> InventoryItemOrder { get; set; }
    }
}
