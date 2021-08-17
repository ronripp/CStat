using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class Inventory
    {
        public Inventory()
        {
            InventoryItem = new HashSet<InventoryItem>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [StringLength(30)]
        public string Name { get; set; }
        public int Type { get; set; }

        [InverseProperty("Inventory")]
        public virtual ICollection<InventoryItem> InventoryItem { get; set; }
    }
}
