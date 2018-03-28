using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using FluentFTP;
using FtpSync.Value;
using NLog;

namespace FtpSync.Real
{
    class FtpLoader : IDisposable
    {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected FtpSettings Settings { get; private set; }
        protected FtpClient Client { get; private set; }
        public string LocalRoot { get; protected set; }
        public string RemoteRoot { get; protected set; }
        public int BrigadeCode { get; protected set; }

        public FtpLoader() { }
        public FtpLoader(FtpSettings settings, int brigadeCode, string remoteRoot, string localRoot)
        {
            this.Settings = settings;
            this.BrigadeCode = brigadeCode;
            this.RemoteRoot = remoteRoot;
            this.LocalRoot = localRoot;
            this.Client = new FtpClient(settings.Ip);
            this.Client.Credentials = new NetworkCredential(settings.User, settings.Password);
        }

        public static FtpLoader Start(FtpSettings settings, int brigadeCode, string remoteRoot, string localRoot)
        {
            var res = new FtpLoader(settings, brigadeCode, remoteRoot, localRoot);
            res.Connect();
            return res;
        }

        public void Connect()
        {
            Client.Connect();
        }

        public bool Download(string localPath, string remotePath)
        {
            return Client.DownloadFile(localPath, remotePath);
        }

        public void DownloadFilesByInterval(DateTime start, DateTime end)
        {
            // Получаем все каталоги отфильтрованные по диапозону дат [час]
            List<RemoteFolder> remoteFolders = RemoteFolder.GetAllHoursFolders(Client, RemoteRoot, folder => folder.BitwinDate(start, end));
           
            foreach (RemoteFolder remoteFolder in remoteFolders)
            {
                // Папка куда копировать файлы [час]
                string localFolder = remoteFolder.GetLocalPath(LocalRoot, BrigadeCode);
                foreach (FtpListItem remoteFile in Client.GetListing(remoteFolder.ToString()))
                {
                    if (remoteFile.Type == FtpFileSystemObjectType.File)
                    {
                        string localFile = Path.Combine(localFolder, remoteFile.Name);
                        IFile f = new FileFactory().Create(localFile);

                        // Файл полностью записан и его нет на диске существует
                        if (f.IsComplete && !System.IO.File.Exists(localFile))
                        {
                            Client.DownloadFile(localFile, remoteFile.FullName);
                            logger.Info($"{f} [OK]");
                        }
                        else
                        {
                            logger.Info($"{f} [MISS]");
                        }
                    }
                }
            }
        }

        public void Disconnect()
        {
            Client.Disconnect();
            Client.Dispose();
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
