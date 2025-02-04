﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using CTask = CStat.Models.Task;
using Task = System.Threading.Tasks.Task;
using CStat.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using static CStat.Common.EquipProp;
using Microsoft.AspNetCore.Identity;
using CStat.Areas.Identity.Data;

namespace CStat.Pages
{
    public class EquipModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private IWebHostEnvironment _hostEnv;
        private readonly ArdMgr _ardMgr;
        private readonly IConfiguration _config;
        private ArdRecord _ar;
        private PropaneLevel _pl;
        public CSSettings Settings { get; set; }
        private List<List<double>> ActiveEqHistory { get; set; }
        private RConnMgr _rconnMgr;
        private List<ClientReport> _rconnClients;
        private bool _rConnsValid = false;

        public EquipModel(CStat.Models.CStatContext context, IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager)
        {
            _context = context;
            _hostEnv = hostEnv;
            _ardMgr = new ArdMgr(_hostEnv, _config, userManager);
            _config = config;
            Settings = CSSettings.GetCSSettings(_config, userManager);
            PropaneMgr pmgr = new PropaneMgr(_hostEnv, _config, userManager);
            _ar = _ardMgr.GetLast();
            if (_ar == null)
                _ar = new ArdRecord(PropMgr.NotSet, PropMgr.NotSet, PropMgr.NotSet, 0, PropMgr.ESTNow);
            _pl = pmgr.GetTUTank(false);
            if (_pl == null)
                _pl = new PropaneLevel(0, PropMgr.ESTNow, PropMgr.NotSet);

            ActiveEqHistory = new List<List<double>>();
            int NumActive = Settings.ActiveEquip.Count;
            if (NumActive > 0)
            {
                List<ArdRecord> ardHist = _ardMgr.GetAll();
                if (ardHist == null)
                    ardHist = new List<ArdRecord>();
                List<PropaneLevel> propaneHist = pmgr.GetAll(30);
                if (propaneHist == null)
                    propaneHist = new List<PropaneLevel>();

                // Ensure latest is at end of list
                int ahCount = ardHist.Count;
                if (ahCount > 0)
                {
                    if (ardHist[ahCount - 1].TimeStamp != _ar.TimeStamp)
                    {
                        if (ardHist[ahCount - 2].TimeStamp == _ar.TimeStamp)
                            _ar = _ardMgr.GetLast(); // get latest
                        else
                        {
                            ardHist.Add(_ar); // set latest
                            if (ahCount >= ArdMgr.MAX_USE_ARS)
                                ardHist.RemoveAt(0);
                        }
                    }
                }
                int plCount = propaneHist.Count;
                if (plCount > 0)
                {
                    if (_pl.ReadingTime > propaneHist[plCount - 1].ReadingTime)
                    {
                        propaneHist.Add(_pl); // set latest
                        if (plCount >= PropaneMgr.MAX_USE_PLS)
                            propaneHist.RemoveAt(0);
                        pmgr.WritePropane(_pl);
                    }
                }

                foreach (var ar in Settings.ActiveEquip)
                {
                    List<double> dlist;
                    dlist = ar.PropName switch
                    {
                        "freezerTemp" => ardHist.Select(a => a.FreezerTempF).Reverse().ToList(),
                        "frigTemp" => ardHist.Select(a => a.FridgeTempF).Reverse().ToList(),
                        "kitchTemp" => ardHist.Select(a => a.KitchTempF).Reverse().ToList(),
                        "propaneTank" => propaneHist.Select(p => p.LevelPct).Reverse().ToList(),
                        "waterPres" => ardHist.Select(a => a.WaterPress).Reverse().ToList(),
                        "powerOn" => ardHist.Select(a => a.PowerOn).Reverse().ToList(),
                        "analog5" => ardHist.Select(a => a.Analog5).Reverse().ToList(),
                        _ => new List<double>()
                    };
                    ActiveEqHistory.Add(dlist);
                }
            }

            _rconnMgr = new RConnMgr(_hostEnv, _context);
            _rconnClients = _rconnMgr.GetValidLast(out _rConnsValid);
        }

        public string GetRConnsColor()
        {
            if (!_rConnsValid)
                return "lightgray";
            if (Event.IsEventDay(_context, false, -8, 6))
                return "lawngreen";
            return (_rconnClients.Count > 0) ? "red" : "lawngreen";
        }
        public string GetRConnsText()
        {
            if (!_rConnsValid)
                return "?";
            return _rconnClients.Count.ToString();
        }

        public ArdRecord GetLastArd()
        {
            if (_ar == null)
                _ar = new ArdRecord(PropMgr.NotSet, PropMgr.NotSet, PropMgr.NotSet, 0, PropMgr.ESTNow);
            return _ar;
        }

        public PropaneLevel GetLastTUTank()
        {
            return _pl;
        }
        public string GetColorClass(string propStr)
        {
            return CSSettings.GetColor(Settings.ActiveEquip, propStr, _ar, _pl, true);
        }

        public List<EquipProp> ActEq { get{return Settings.ActiveEquip;} }

        public string ActEqVal(int i)
        {
            EquipProp ep = Settings.ActiveEquip[i];
            if (ep == null)
                return "???";
            return ep.PropName switch
            {
                "freezerTemp" => _ar.FreezerTempF.ToString("0.#"),
                "frigTemp" => _ar.FridgeTempF.ToString("0.#"),
                "kitchTemp" => _ar.KitchTempF.ToString("0.#"),
                "propaneTank" => _pl.LevelPct.ToString("0.#"),
                "waterPres" => _ar.WaterPress.ToString("0.#"),
                "powerOn" => _ar.PowerOn.ToString("0.#"),
                "analog5" => _ar.Analog5.ToString("0.#"),
                _ => "---",
            };
        }

        public string ActEqUnits(int i)
        {
            EquipProp ep = Settings.ActiveEquip[i];
            if (ep == null)
                return "???";
            return ep.EquipUnits switch
            {
                EquipUnitsType.TemperatureF => "F",
                EquipUnitsType.TemperatureC => "C",
                EquipUnitsType.PSI => "psi",
                EquipUnitsType.PercentFull => "%",
                EquipUnitsType.KWH => "KWh",
                _ => "",
            };
        }
        public string ActEqDT(int i)
        {
            EquipProp ep = Settings.ActiveEquip[i];
            if (ep == null)
                return "???";
            return ep.PropName switch
            {
                "freezerTemp" => _ar.TimeStampStr(),
                "frigTemp" => _ar.TimeStampStr(),
                "kitchTemp" => _ar.TimeStampStr(),
                "propaneTank" => _pl.ReadingTimeStr(),
                "waterPres" => _ar.TimeStampStr(),
                "powerOn" => _ar.TimeStampStr(),
                "analog5" => _ar.TimeStampStr(),
                _ => "---",
            };
        }

        public List<double> ActEqHistory(int i)
        {
            return ActiveEqHistory[i]; // in HTML, first is most recent : start at Width = 100 % and go backwards till empty.
        }

    }
}
