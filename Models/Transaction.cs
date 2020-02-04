using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class Transaction
    {
        public Transaction()
        {
            Attendance = new HashSet<Attendance>();
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
    }
}
