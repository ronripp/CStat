using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dropbox.Api;
using CStat.Data;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace CStat
{
    public class DocsModel : PageModel
    {
        public CSDropBox dbox;
        private IWebHostEnvironment hostEnv;
        //string DropBoxFile = null;
        //Dictionary<string, string> UrlMap = null;

        private string _FolderName = "";
        public DocsModel(IWebHostEnvironment hstEnv)
        {
            hostEnv = hstEnv;
            dbox = new CSDropBox(Startup.CSConfig);
        }

        public void OnGet(string id)
        {
            FolderName = id ?? "";
            dbox.GetFolderList(FolderName);
            ViewData["Title"] = "Docs" + FolderName;

            //if (FolderName.Length > 0) { 
            //    DropBoxFile = Path.Combine(hostEnv.WebRootPath, "DropBox_" + FolderName);
            //    UrlMap = new Dictionary<string, string>();

            //using (StreamReader sr = new StreamReader(DropBoxFile))
            //{
            //    while (sr.Peek() >= 0)
            //    {
            //        string[] fields = sr.ReadLine().Split('~');
            //        if (fields.Length >= 2)
            //        {
            //            UrlMap.Add(fields[0], fields[1]);
            //        }
            //    }
            //}
        }

        public string GetSharedFileUrl (string file)
        {
            return dbox.GetSharedLink(FolderName + "/" + file);
        }

        public string FolderName {get{return _FolderName;} set{_FolderName=value;}}
    }
}