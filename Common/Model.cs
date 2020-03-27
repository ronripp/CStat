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
            Basement_Shelves = 5,
            Basement_Closet = 6,
            Laundry_Room = 7,
            Basement_Tanks = 8,
            Basement_Heater = 9
        };
    }

    public partial class Person
    {
        public enum eGender
        {
            Male = (int)(uint)(byte)'M',
            Female = (int)(uint)(byte)'F'
        };

        public enum eSkills
        { 
            Carpentry    = 0x1,
            Plumbing     = 0x2,   
            Roofing      = 0x4,   
            Cook         = 0x8,   
            Nurse_Health = 0x10,  
            SWAT         = 0x20,  
            CampMgr      = 0x40,  
            Painter      = 0x80,  
            Minister     = 0x100, 
            Dean         = 0x200, 
            Accoutant    = 0x400, 
            Electrician  = 0x800, 
            Computer     = 0x1000,
            Kitchen_Help = 0x2000,
            Counselor    = 0x4000,
            Gardening    = 0x8000,
            Tree_Remove  = 0x10000,
            Worship      = 0x20000, 
            Worker       = 0x40000, 
            Mason        = 0x80000,
            Construction = 0x100000,
            Septic_Drain = 0x200000
        };
    }
}