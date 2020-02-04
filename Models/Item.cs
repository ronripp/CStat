using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class Item
    {
        public Item()
        {
            InventoryItem = new HashSet<InventoryItem>();
        }

        [Required]
        [Column("name")]
        [StringLength(80)]
        public string Name { get; set; }
        [Column("UPC")]
        [StringLength(12)]
        public string Upc { get; set; }
        [Column("Mfg_id")]
        public int? MfgId { get; set; }
        public float Size { get; set; }
        public int Units { get; set; }
        public int? Status { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey(nameof(MfgId))]
        [InverseProperty(nameof(Business.Item))]
        public virtual Business Mfg { get; set; }
        [ForeignKey(nameof(MfgId))]
        [InverseProperty(nameof(Manufacturer.Item))]
        public virtual Manufacturer MfgNavigation { get; set; }
        [InverseProperty("IdNavigation")]
        public virtual TransactionItems TransactionItems { get; set; }
        [InverseProperty("Item")]
        public virtual ICollection<InventoryItem> InventoryItem { get; set; }
    }
}
