using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dropbox.Api;
using CStat.Data;
using System.Web;

namespace CStat
{
    public class DocsModel : PageModel
    {
        public CSDropBox dbox;
        private string _FolderName = "";
        public DocsModel()
        {
            dbox = new CSDropBox();
        }

        public void OnGet(string id)
        {
            FolderName = id ?? "";
            dbox.GetFolderList(FolderName);
            ViewData["Title"] = "Docs" + FolderName;
        }

        public string GetSharedFileUrl (string file)
        {
            return dbox.GetSharedLink(FolderName + "/" + file);
        }

        public string FolderName {get{return _FolderName;} set{_FolderName=value;}}
    }
}