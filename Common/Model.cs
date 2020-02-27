using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CStat.Models
{
    public partial class InventoryItem
    {
        public enum States { InStock = 0, OpenNeed = 1, TakenNeed = 2 };
        public enum ItemUnits
        {
            unknown = 0,
            bags = 1,
            bladders = 2,
            bottles = 3,
            boxes = 4,
            bulbs = 5,
            drums = 6,
            jugs = 7,
            ounces = 8,
            pairs = 9,
            pieces = 10,
            reams = 11,
            rolls = 12,
            sheets = 13,
            tablets = 14
        };

        public enum ItemZone
        {
            unknown = 0,
            Office = 1,
            Kitchen_Front = 2,
            Kitchen_Mid = 3,
            Kitchen_Back = 4,
            Basement_Stairs = 5,
            Basement_Water = 6,
        };

    }
}