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
using CStat.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using CStat.Areas.Identity.Data;
//using Microsoft.AspNetCore.Cors;

namespace CStat
{
    public class DocsModel : PageModel
    {
        public CSDropBox dbox;
        private IWebHostEnvironment hostEnv;
        public String DescStr = "";
        public bool IsSelect = false;
        public string SelectStr = "";
        private string DescFile = null;
        private Dictionary<string, string> DescMap=null;
        private const String RepStr = "~";

        private string _FolderName = "";
        public DocsModel(IWebHostEnvironment hstEnv, IHttpContextAccessor httpContextAccessor, IConfiguration config, UserManager<CStatUser> userManager)
        {
            hostEnv = hstEnv;
            var csSettings = CSSettings.GetCSSettings(config, userManager);
            string UserId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Subject.Name;
            var curUser = csSettings.GetUser(UserId);
            dbox = new CSDropBox(Startup.CSConfig, (curUser == null || curUser.IsFull));
        }

        public void OnGet(string id, string selectStr)
        {
            FolderName = id ?? "";
            if (!string.IsNullOrEmpty(selectStr))
            {
                SelectStr = selectStr.Trim();
                IsSelect = SelectStr.Length > 1;
            }
            else
            {
                SelectStr = "";
                IsSelect = false;
            }

            dbox.GetFolderList(FolderName);
            ViewData["Title"] = "Docs" + FolderName;
            FillDescMap(FolderName);
        }

        public string GetFolderNameWL()
        {
            // <b><a href="Docs?id=@Uri.EscapeDataString(Model.FolderName + "/" + item.Name)&selectStr=@SelStr">@Html.DisplayFor(modelItem => item.Name)</a></b>

            var fList = _FolderName.Split("/").Where(s => !string.IsNullOrWhiteSpace(s.Trim())).ToList();
            int cnt = fList.Count();
            if (cnt == 0)
                return "";
            string fnwl = "<b>";
            if (cnt > 1)
            { 
                string curPath = "";
                for (int i = 0; i < cnt - 1; ++i)
                {
                    string f = fList[i];
                    curPath += "/" + f;
                    fnwl += "<a href=\"Docs?id=" + Uri.EscapeDataString(curPath) + "\">" + f + "></a>";
                }
            }
            fnwl += fList[cnt - 1];
            return fnwl + "</b>";
        }

        private void FillDescMap(string fldName)
        {

            if (DescMap == null)
            {
                DescMap = new Dictionary<string, string>();
                SetDescFile(fldName);
                try
                {
                    // Read Desc File.
                    using (StreamReader sr = new StreamReader(DescFile))
                    {
                        while (sr.Peek() >= 0)
                        {
                            string[] fields = sr.ReadLine().Split('|');
                            if (fields.Length >= 2)
                            {
                                DescMap.Add(fields[0], fields[1]);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                }
            }
        }
        private void SetDescFile(string fld)
        {
            string fldName = Uri.UnescapeDataString(fld);
            if (fldName.Length == 0)
                fldName = RepStr;
            DescFile = Path.Combine(hostEnv.WebRootPath, "DropBox_" + "\\" + fldName.Replace("/", RepStr));
        }
        private void WriteDescMap(string FolderName)
        {
            SetDescFile(FolderName);
            try
            {
                // Write Desc File
                using (StreamWriter sw = new StreamWriter(DescFile))
                {
                    foreach (var kvp in DescMap)
                    {
                        sw.WriteLine(kvp.Key + "|" + kvp.Value);
                    }
                }
            }
            catch            {
                
            }
        }

        public string GetSharedFileUrl (string file)
        {
            return dbox.GetSharedLink(FolderName + "/" + file);
        }

        public string GetDescFromFile(string file)
        {
            if (DescMap.TryGetValue(file, out string Desc)) {
                return Desc;
            }
            return "[Add]";
        }

        public string FolderName {get{return _FolderName;} set{_FolderName=value;}}

        public JsonResult OnGetFileShare()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            Dictionary<string, string> NVPairs  = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonQS);
            
            if (NVPairs.TryGetValue("Folder", out string FolderName) && NVPairs.TryGetValue("File", out string FileName))
            {
                if (FileName.EndsWith(".txt", StringComparison.CurrentCultureIgnoreCase) ||
                    FileName.EndsWith(".pdf", StringComparison.CurrentCultureIgnoreCase) ||
                    FileName.EndsWith(".docx", StringComparison.CurrentCultureIgnoreCase) ||
                    FileName.EndsWith(".doc", StringComparison.CurrentCultureIgnoreCase) ||
                    FileName.EndsWith(".htm", StringComparison.CurrentCultureIgnoreCase) ||
                    FileName.EndsWith(".html", StringComparison.CurrentCultureIgnoreCase))
                {
                    FileName = UnencodeQuotes(FileName);
                    string destFName = Path.Combine("tmpDBox", (FolderName + FileName).Replace('/', '~').Replace('\\', '~'));
                    string destFile = Path.Combine(hostEnv.WebRootPath, destFName);
                    
                    if (System.IO.File.Exists(destFile))
                        System.IO.File.Delete(destFile);
                    try
                    {
                        var task = dbox.DownloadToFile(FolderName, FileName, destFile);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        return new JsonResult("ERROR~:" + ex.Message ?? "Failed to download");
                    }
                    return new JsonResult(destFName);
                }
                string url = dbox.GetSharedLink(Uri.UnescapeDataString(UnencodeQuotes(FolderName) + "/" + UnencodeQuotes(FileName)));

                if (url.Length > 0)
                    return new JsonResult(url);
                else
                    return new JsonResult("ERROR~:No File Share");
            }
            return new JsonResult("ERROR~:Incorrect Parameters");
        }

        public string EncodeQuotes(string str)
        {
            return (str.Length > 0) ? str.Replace("\"", "^^").Replace("'", "^") : "";
        }
        private string UnencodeQuotes(string str)
        {
            return (str.Length > 0) ? str.Replace("^^", "\"").Replace("^", "'") : "";
        }
        public JsonResult OnGetFileDesc()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            Dictionary<string, string> NVPairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonQS);
            if (NVPairs.TryGetValue("Folder", out string FolderName) && NVPairs.TryGetValue("File", out string FileName) && NVPairs.TryGetValue("Desc", out string FileDesc))
            {
                if (FileDesc.Length > 0)
                {
                    FillDescMap(FolderName);
                    DescMap[FileName] = UnencodeQuotes(FileDesc);
                    WriteDescMap(FolderName);
                    return new JsonResult("SUCCESS~:Description updated.");
                }
            }
            return new JsonResult("ERROR~:Incorrect Parameters");
        }

        public JsonResult OnGetUnload()
        {
            return new JsonResult("SUCCESS~:OK");
        }

    }
}