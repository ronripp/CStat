using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CStat.Models
{
    public partial class Item
    {
        public enum ItemGroup { Baking_Spice = 1, Dairy = 2, Starch_Bread = 3, Canned_Shelf_Safe = 4, Condiments = 5, Frozen = 6, Veg_Fruit = 7 };

        // Simple read-write property
        public ItemGroup Group {
            get { return (ItemGroup)((Status & 0x7E) >> 1); }
            set { if (!Status.HasValue)
                    Status = 0;
                Status = ((int)Status.Value & 1) | ((int)value << 1); }
        }

        // TBD public IsFood 

        // Auto-property with a default fallback value
        public int Score { get; set; } = 100;


    }

}
