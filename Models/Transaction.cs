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
        [Column("id")]
        public int Id { get; set; }
        [Column("Income_Type")]
        public int? IncomeType { get; set; }
        [Column("Expense_Type")]
        public int? ExpenseType { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? Amount { get; set; }
        [Column("Person_id")]
        public int? PersonId { get; set; }
        [Column("Business_id")]
        public int? BusinessId { get; set; }
        [Column("Church_id")]
        public int? ChurchId { get; set; }
        [Column(TypeName = "datetime2(0)")]
        public DateTime Date { get; set; }
        [Column("CCA_Account_id")]
        public int CcaAccountId { get; set; }
        [Column("Source_Check_Num")]
        public int? SourceCheckNum { get; set; }
        [Column("memo")]
        [StringLength(50)]
        public string Memo { get; set; }

        [ForeignKey(nameof(BusinessId))]
        [InverseProperty("Transaction")]
        public virtual Business Business { get; set; }
        [ForeignKey(nameof(CcaAccountId))]
        [InverseProperty(nameof(Account.Transaction))]
        public virtual Account CcaAccount { get; set; }
        [ForeignKey(nameof(ChurchId))]
        [InverseProperty("Transaction")]
        public virtual Church Church { get; set; }
        [ForeignKey(nameof(PersonId))]
        [InverseProperty("Transaction")]
        public virtual Person Person { get; set; }
        [InverseProperty("Transaction")]
        public virtual ICollection<Attendance> Attendance { get; set; }
    }
}
