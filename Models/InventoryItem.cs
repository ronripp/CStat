using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class InventoryItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [Column("Item_id")]
        public int ItemId { get; set; }
        [Column("Inventory_id")]
        public int InventoryId { get; set; }
        [Column("Current_Stock")]
        public float? CurrentStock { get; set; }
        [Column("Reorder_Threshold")]
        public float? ReorderThreshold { get; set; }
        public int? Units { get; set; }
        [Column(TypeName = "datetime2(0)")]
        public DateTime? Date { get; set; }
        [Column("Order_Id")]
        public int? OrderId { get; set; }
        [Column("Person_Id")]
        public int? PersonId { get; set; }
        public int? State { get; set; }
        [Column("Units_per_day")]
        public double? UnitsPerDay { get; set; }
        public int? Zone { get; set; }

        [ForeignKey(nameof(InventoryId))]
        [InverseProperty("InventoryItem")]
        public virtual Inventory Inventory { get; set; }
        [ForeignKey(nameof(ItemId))]
        [InverseProperty("InventoryItem")]
        public virtual Item Item { get; set; }
        [ForeignKey(nameof(OrderId))]
        [InverseProperty(nameof(Transaction.InventoryItem))]
        public virtual Transaction Order { get; set; }
        [ForeignKey(nameof(PersonId))]
        [InverseProperty("InventoryItem")]
        public virtual Person Person { get; set; }
    }
}
