using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentFTP;
using FtpSync.Value;

namespace FtpSync.Real
{
    abstract class FtpLoader : IDisposable
    {
        protected readonly FtpSettings settings;
        protected FtpClient client;
        protected readonly string localRoot;
        protected readonly string remoteRoot;

        protected FtpLoader(FtpSettings settings, string localRoot, string remoteRoot)
        {
            this.settings = settings;
            this.localRoot = localRoot;
            this.remoteRoot = remoteRoot;
        }

        public void Connect()
        {
            client = new FtpClient(settings.Ip);
            client.Credentials = new NetworkCredential(settings.User, settings.Password);
            client.Connect();
        }

        public bool Download(string localPath, string remotePath)
        {
            return client.DownloadFile(localPath, remotePath);
        }

        //public bool DownloadByPeriod(DateTime start, DateTime end)
        //{
        //    return client.DownloadFile(localPath, remotePath);
        //}

        //public List<string> GetRemoteFiles(DateTime start, DateTime end)
        //{
           
        //}



        public void Disconnect()
        {
            client.Disconnect();
        }

        public void Dispose()
        {
            Disconnect();
        }
    }


}
