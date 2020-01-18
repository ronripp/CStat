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
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace CStat
{
    public class DocsModel : PageModel
    {
        public CSDropBox dbox;
        private IWebHostEnvironment hostEnv;
        public String DescStr = "";
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

        public async Task<IActionResult> OnPostAsync()
        {
            //foreach (KeyValuePair<string,StringValues> kvp in Request.Form)
            //{
            //    if (kvp.Key.StartsWith("/"))
            //    {
            //        string FolderName = kvp.Key;
            //        string FileName = kvp.Value[0];
            //        string url = dbox.GetSharedLink(Uri.UnescapeDataString(FolderName + "/" + FileName));
            //        if (url.Length > 0)
            //        {
            //            Redirect(url);
            //            return StatusCode(200);
            //        }
            //        else
            //        {
            //            RedirectToPage("Docs?id=" + Uri.EscapeDataString(FolderName));
            //            return StatusCode(200);
            //        }                 
            //    }
            //}
            return StatusCode(200);
        }
        public JsonResult OnGetFileShare()
        { 
            var rawQS = Request.QueryString.ToString();
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            Dictionary<string, string> NVPairs  = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonQS);
            if (NVPairs.TryGetValue("Folder", out string FolderName) && NVPairs.TryGetValue("File", out string FileName))
            {
                string url = dbox.GetSharedLink(Uri.UnescapeDataString(FolderName + "/" + FileName));
                if (url.Length > 0)
                    return new JsonResult(url);
                else
                    return new JsonResult("ERROR~:No File Share");
            }
            return new JsonResult("ERROR~:Incorrect Parameters");
        }
        public JsonResult OnGetFileDesc()
        {
            var rawQS = Request.QueryString.ToString();
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            Dictionary<string, string> NVPairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonQS);
            if (NVPairs.TryGetValue("Folder", out string FolderName) && NVPairs.TryGetValue("File", out string FileName) && NVPairs.TryGetValue("Desc", out string FileDesc))
            {
                var key = FolderName + "/" + FileName;
                return new JsonResult("SUCCESS~:Description updated."); 
            }
            return new JsonResult("ERROR~:Incorrect Parameters");
        }
    }
}