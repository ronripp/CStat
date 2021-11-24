using CStat.Areas.Identity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class CmdMgr
    {
        enum CmdSource
        {
            INVENTORY = 0x00000001,
            PROPANE = 0x00000002,
            PERSON = 0x00000004,
            DOC = 0x00000008,
            TASKS = 0x00000010,
        }
        enum CmdAction
        {
            LIST = 0x00010000,
            FIND = 0x00020000,
            COMPUTE = 0x00040000,
            ADD = 0x00080000,
            DELETE = 0x00100000,
            UPDATE = 0x00200000,
        }
        enum CmdDest
        {
            REPLY = 0x10000000,
            EMAIL = 0x20000000
        }

        private static readonly Dictionary<string, CmdSource> CmdSrcDict = new Dictionary<string, CmdSource>
        {
            {"Stock", CmdSource.INVENTORY},
            {"propane", CmdSource.PROPANE},
            {"tank", CmdSource.PROPANE},
            {"fuel", CmdSource.PROPANE},
            {"gas", CmdSource.PROPANE},
            {"inv", CmdSource.INVENTORY},
            {"inventory", CmdSource.INVENTORY},
            {"Supplies", CmdSource.INVENTORY},
        };

        private readonly CStat.Models.CStatContext Context;
        private readonly CSSettings csSettings;
        private readonly IWebHostEnvironment HostEnv;
        private readonly IConfiguration Config;
        private readonly UserManager<CStatUser> UserManager;

        public CmdMgr (CStat.Models.CStatContext context, CSSettings cset, IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager)
        {
            Context = context;
            csSettings = cset;
            HostEnv = hostEnv;
            Config = config;
            UserManager = userManager;
        }
        public string ParseCmd (string[] words)
        {
            if (words[0].ToLower() == "propane")
            {
                var pmgr = new PropaneMgr(HostEnv, Config, UserManager);
                var pl = pmgr.GetTUTank();
                return "Propane read " + pl.LevelPct.ToString("0.#") + "% at " + pl.ReadingTimeStr();
            }
            return "Huh?";
        }

        private CmdSource _cmdSrc;
        private CmdAction _cmdAction;
        private CmdDest _cmdDest;
    }
}
