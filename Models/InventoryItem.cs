using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

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
        [Column("buy1_id")]
        public int? Buy1Id { get; set; }
        [Column("buy2_id")]
        public int? Buy2Id { get; set; }
        [Column("buy3_id")]
        public int? Buy3Id { get; set; }
        [Column("Expected_Cost", TypeName = "decimal(13, 2)")]
        public decimal? ExpectedCost { get; set; }

        [ForeignKey(nameof(Buy1Id))]
        [InverseProperty(nameof(Transaction.InventoryItemBuy1))]
        public virtual Transaction Buy1 { get; set; }
        [ForeignKey(nameof(Buy2Id))]
        [InverseProperty(nameof(Transaction.InventoryItemBuy2))]
        public virtual Transaction Buy2 { get; set; }
        [ForeignKey(nameof(Buy3Id))]
        [InverseProperty(nameof(Transaction.InventoryItemBuy3))]
        public virtual Transaction Buy3 { get; set; }
        [ForeignKey(nameof(InventoryId))]
        [InverseProperty("InventoryItem")]
        public virtual Inventory Inventory { get; set; }
        [ForeignKey(nameof(ItemId))]
        [InverseProperty("InventoryItem")]
        public virtual Item Item { get; set; }
        [ForeignKey(nameof(OrderId))]
        [InverseProperty(nameof(Transaction.InventoryItemOrder))]
        public virtual Transaction Order { get; set; }
        [ForeignKey(nameof(PersonId))]
        [InverseProperty("InventoryItem")]
        public virtual Person Person { get; set; }
    }
}
