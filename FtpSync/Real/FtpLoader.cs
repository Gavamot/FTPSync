using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using FtpSync.Real.File;
using FtpSync.Value;
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

        /// <summary>
        /// Загружает по ftp файлы за переданный интервал 
        /// </summary>
        /// <param name="interval">временной интервал</param>
        /// <param name="remoteRoot">удаленный каталог</param>
        /// <param name="localRoot">куда копировать</param>
        /// <param name="token">Токен для отмены</param>
        /// <returns>Возвращает список скопированных  или существующих на сервере файлов</returns>
        public List<IFile> DownloadFilesByInterval(DateInterval interval, string remoteRoot, string localRoot, CancellationTokenSource cts = null)
        {
            // Получаем все каталоги отфильтрованные по диапозону дат [час]
            List<RemoteFolder> remoteFolders = RemoteFolder.GetAllHoursFolders(Client, remoteRoot, interval.BitwinDateFolder);
            var res = new List<IFile>();
            foreach (RemoteFolder remoteFolder in remoteFolders)
            {
                // Папка куда копировать файлы [час]
                foreach (FtpListItem remoteFile in Client.GetListing(remoteFolder.ToString()))
                {
                    cts?.Token.ThrowIfCancellationRequested();

                    if (remoteFile.Type == FtpFileSystemObjectType.File)
                    {
                        string localFile = remoteFile.FullName.Replace("/", "\\")
                            .Replace(remoteRoot.Replace("/", "\\"), localRoot.Replace("/", "\\")); 

                        // Проверяем файл на соответствие формату
                        IFile f;
                        try
                        {
                            f = FileFactory.Create(remoteFile.Name);
                        }
                        catch (FormatException e)
                        {
                            logger.Error(e, $"{ localFile } haves bad format [ERROR]");
                            continue;
                        }
                         
                        // Копируем файл
                        if (!f.IsComplete) // Файл не полностью записан
                        {
                            logger.Info($" {f} [MISS] - not completed");
                        }
                        else if (!f.IsInInterval(interval)) // Не входит в интервал
                        {
                            //logger.Info($" {f} [MISS] - Exists in the server");
                        }
                        else if(System.IO.File.Exists(localFile)) // Нет в папке на сервере
                        {
                            logger.Info($" {f} [MISS] - exists in the server");
                            res.Add(f);
                        }
                        else //Переносим файл
                        {
                            try
                            {
                                Client.DownloadFile(localFile, remoteFile.FullName);
                                res.Add(f);
                                logger.Info($"{f} [OK]");
                            }
                            catch (Exception e)
                            {
                                logger.Error(e, $"copy {f} [ERROR]");
                            }
                        }

                    }

                }
            }
            return res;
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
