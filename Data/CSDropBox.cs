using Dropbox.Api;
using Dropbox.Api.Sharing;
using Dropbox.Api.Stone;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Dropbox.Api.Files;

namespace CStat.Data
{
    public class CSDropBox
    {
        // Note : Dropbox path starts with /
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

        // Upload a file as stream :
        public async Task UploadStringToFileAsync(string folder, string file, string content)
        {
            using (var mem = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
            {
                var updated = await dbx.Files.UploadAsync(
                    folder + "/" + file,
                    Dropbox.Api.Files.WriteMode.Overwrite.Instance,
                    body: mem);
            }
        }

        // Upload a file:
        public async Task<Metadata> UploadFileAsync(string srcFile, string destFolder, string destFile)
        {
            FileStream fs = new FileStream(srcFile, FileMode.Open, FileAccess.Read);
            var updated = await dbx.Files.UploadAsync(
                destFolder + "/" + destFile,
                Dropbox.Api.Files.WriteMode.Overwrite.Instance,
                body: fs);
            fs.Close();
            return updated;
        }

        public bool UploadFile(string srcFile, string destFolder, string destFile)
        {
            var upTask = UploadFileAsync(srcFile, destFolder, destFile);
            upTask.Wait();
            return upTask.Result != null;
        }

        public bool FileExists(string path)
        {
            var feTask = FileExistsAsync(path);
            feTask.Wait();
            return feTask.Result;
        }

        public async Task<bool> FileExistsAsync(string path)
        {
            try
            {
                Metadata res = await dbx.Files.GetMetadataAsync(path);
                Console.WriteLine("{0}", res);
            }
            catch (ApiException<GetMetadataError> e)
            {
                if (e.ErrorResponse.IsPath)
                {
                    if (e.ErrorResponse.AsPath.Value.IsNotFound)
                    {
                        Console.WriteLine("Path not found!");
                    }
                    else if (e.ErrorResponse.AsPath.Value.IsMalformedPath)
                    {
                        Console.WriteLine("Malformed path!");
                    } // and so on for the other .Is* methods as desired
                    else
                    {
                        Console.WriteLine("LookupError: {0}", e.ErrorResponse.AsPath.Value);
                    }
                }
                else
                {
                    Console.WriteLine("GetMetadataError: {0}", e);
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Some other error: {0}", e);
            }
            return true;
        }

    }
}

