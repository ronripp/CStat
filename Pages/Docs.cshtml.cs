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
        private string DescFile = null;
        private Dictionary<string, string> DescMap=null;
        private const String RepStr = "~";

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
            FillDescMap(FolderName);
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
                catch (IOException e)
                {
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
            catch (IOException e)
            {
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
                    FileName.EndsWith(".htm", StringComparison.CurrentCultureIgnoreCase) ||
                    FileName.EndsWith(".html", StringComparison.CurrentCultureIgnoreCase))
                {
                    string destFName = Path.Combine("tmpDBox", (FolderName + FileName).Replace('/', '~').Replace('\\', '~'));
                    string destFile = Path.Combine(hostEnv.WebRootPath, destFName);
                    
                    System.IO.File.Delete(destFile);
                    var task = dbox.DownloadToFile(FolderName, FileName, destFile);
                    task.Wait();
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