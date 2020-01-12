using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class InventoryItem
    {
        [Column("Item_id")]
        public int ItemId { get; set; }
        [Column("Inventory_id")]
        public int InventoryId { get; set; }
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("Current_Stock")]
        public float? CurrentStock { get; set; }
        [Column("Reorder_Threshold")]
        public float? ReorderThreshold { get; set; }
        public int? Units { get; set; }

        [ForeignKey(nameof(InventoryId))]
        [InverseProperty("InventoryItem")]
        public virtual Inventory Inventory { get; set; }
        [ForeignKey(nameof(ItemId))]
        [InverseProperty("InventoryItem")]
        public virtual Item Item { get; set; }
    }
}
