using Dropbox.Api;
using Dropbox.Api.Sharing;
using Dropbox.Api.Stone;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CStat.Data
{
    public class CSDropBox
    {
        public DropboxClient dbx = null;
        public Dropbox.Api.Files.ListFolderResult FolderList = null;
        public CSDropBox(IConfiguration configuration)
        {
            Configuration = configuration;
            var task = Task.Run((Func<Task>)Run);
            task.Wait();
        }
        public IConfiguration Configuration { get; }
        private async Task Run()
        {
            dbx = new DropboxClient(Configuration.GetValue<string>("CSDropBox:ShareFileKey"));
            var full = await dbx.Users.GetCurrentAccountAsync();
            Console.WriteLine("{0} - {1}", full.Name.DisplayName, full.Email);
        }

        // Get Folder List
        public int GetFolderList (string folder)
        {
            FolderList = dbx.Files.ListFolderAsync(folder).Result;
            return FolderList.Entries.Count();
        }

        public string GetSharedLink(string path)
        {
//            if (path.StartsWith(@"/Getting")) return "";

            SharedLinkMetadata sharedLinkMetadata=null;
            try
            {
                var sharedLinksMetadata = dbx.Sharing.ListSharedLinksAsync(path, null, true).Result;
                sharedLinkMetadata = sharedLinksMetadata.Links.First();
            }
            catch
            {
                try
                {
                    sharedLinkMetadata = dbx.Sharing.CreateSharedLinkWithSettingsAsync(path).Result;
                }
                catch
                {
                    return "";
                }
            }
            return sharedLinkMetadata.Url;

            //IDownloadResponse<SharedLinkMetadata> dr = null;
            //SharedLinkMetadata slm = null;
            //try
            //{
            //    dr = dbx.Sharing.GetSharedLinkFileAsync(path).Result;
            //}
            //catch
            //{
            //    try
            //    {
            //        slm = dbx.Sharing.CreateSharedLinkWithSettingsAsync(path).Result;
            //    }
            //    catch
            //    {
            //        return slm.Url;
            //    }
            //    return slm.Url;
            //}
            //if (dr.Response.Url != null)
            //    return dr.Response.Url;
            //return "";
        }

        // List all the contents in the user's root directory:
        public async Task ListRootFolder()
        {
            var list = await dbx.Files.ListFolderAsync("");

            // show folders then files
            foreach (var item in list.Entries.Where(i => i.IsFolder))
            {
                Console.WriteLine("D  {0}/", item.Name);
            }

            foreach (var item in list.Entries.Where(i => i.IsFile))
            {
                Console.WriteLine("F{0,8} {1}", item.AsFile.Size, item.Name);
            }
        }

        // Download a file to destPath:
        public async Task DownloadToFile(string folder, string file, string destPath)
        {
            using (var response = await dbx.Files.DownloadAsync(folder + "/" + file))
            {
                using (var fileStream = File.Create(destPath))
                {
                    (await response.GetContentAsStreamAsync()).CopyTo(fileStream);
                }
            }
        }

        // Upload a file:
        public async Task Upload(string folder, string file, string content)
        {
            using (var mem = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
            {
                var updated = await dbx.Files.UploadAsync(
                    folder + "/" + file,
                    Dropbox.Api.Files.WriteMode.Overwrite.Instance,
                    body: mem);
                Console.WriteLine("Saved {0}/{1} rev {2}", folder, file, updated.Rev);
            }
        }
    }

}

