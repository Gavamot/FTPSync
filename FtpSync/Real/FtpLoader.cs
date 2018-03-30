using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using FluentFTP;
using FtpSync.Value;
using Ninject;
using NLog;

namespace FtpSync.Real
{
    class FtpLoader : IDisposable
    {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected FtpSettings Settings { get; private set; }
        protected FtpClient Client { get; private set; }

        public FtpLoader() { }
        public FtpLoader(FtpSettings settings)
        {
            this.Settings = settings;
            this.Client = new FtpClient(settings.Ip);
            this.Client.Credentials = new NetworkCredential(settings.User, settings.Password);
        }

        public static FtpLoader Start(FtpSettings settings)
        {
            var res = new FtpLoader(settings);
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

        public void DownloadFilesByInterval(DateTimeInterval interval, string remoteRoot, string localRoot)
        {
            // Получаем все каталоги отфильтрованные по диапозону дат [час]
            List<RemoteFolder> remoteFolders = RemoteFolder.GetAllHoursFolders(Client, remoteRoot, interval.BitwinDate );
            foreach (RemoteFolder remoteFolder in remoteFolders)
            {
                // Папка куда копировать файлы [час]
                foreach (FtpListItem remoteFile in Client.GetListing(remoteFolder.ToString()))
                {
                    if (remoteFile.Type == FtpFileSystemObjectType.File)
                    {
                        string localFile = remoteFile.FullName.Replace("/", "\\")
                            .Replace(remoteRoot.Replace("/", "\\"), localRoot.Replace("/", "\\")); 
                        // Проверяем файл на соответствие формату
                        IFile f = new FileChannelJson();
                        try
                        {
                            f = new FileFactory().Create(remoteFile.Name);
                        }
                        catch (FormatException e)
                        {
                            logger.Error(e, $"{ localFile } haves bad format [ERROR]");
                            continue;
                        }

                        // Файл полностью записан и его нет на диске существует
                        if (f.IsComplete && !System.IO.File.Exists(localFile))
                        {
                            try
                            {
                                Client.DownloadFile(localFile, remoteFile.FullName);
                                logger.Info($"{f} [OK]");
                            }
                            catch (Exception e)
                            {
                                logger.Error(e, $"copy {f} [ERROR]");
                            }
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
