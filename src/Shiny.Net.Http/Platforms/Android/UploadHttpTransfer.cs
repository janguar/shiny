﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace Shiny.Net.Http
{
    public class UploadHttpTransfer : AbstractHttpTransfer
    {
        readonly HttpClient httpClient;
        readonly CancellationTokenSource cancelSrc;


        public UploadHttpTransfer(HttpTransferRequest request) : base(request, true)
        {
            this.httpClient = new HttpClient();
            this.cancelSrc = new CancellationTokenSource();
            //this.Identifier = identifier;

            //this.GetManager().Remove(id);
        }


        //Task Run() => Task.Run(async () =>
        //{
        //    if (this.IsUpload)
        //    {
        //        await this.DoUpload().ConfigureAwait(false);
        //    }
        //    else
        //    {
        //        await this.DoDownload().ConfigureAwait(false);
        //    }
        //});


        //public override void Cancel()
        //{
        //    this.Status = HttpTransferState.Cancelled;
        //    this.cancelSrc.Cancel();
        //    this.httpClient.CancelPendingRequests();
        //}


        async Task DoUpload()
        {
            var lfp = this.Request.LocalFile;
            if (!lfp.Exists)
                throw new ArgumentException($"Local '{lfp.FullName}' file does not exist");

            this.FileSize = lfp.Length;
            this.RemoteFileName = lfp.Name;

            while (this.Status != HttpTransferState.Completed && !this.cancelSrc.IsCancellationRequested)
            {
                try
                {
                    var content = new MultipartFormDataContent();
                    content.Add(
                        new ProgressStreamContent(
                            lfp.OpenRead(),
                            8192,
                            sent =>
                            {
                                this.Status = HttpTransferState.Running;
                                this.BytesTransferred += sent;
                                this.RunCalculations();
                            }
                        ),
                        "blob",
                        this.RemoteFileName
                    );

                    await this.httpClient.PostAsync(this.Request.Uri, content, this.cancelSrc.Token);
                    this.Status = this.cancelSrc.IsCancellationRequested
                        ? HttpTransferState.Cancelled
                        : HttpTransferState.Completed;
                }
                catch (IOException ex)
                {
                    if (ex.InnerException is WebException)
                    {
                        this.Status = HttpTransferState.Retrying;
                    }
                    else
                    {
                        this.Exception = ex;
                    }
                }
                catch (TaskCanceledException)
                {
                    this.Status = this.cancelSrc.IsCancellationRequested
                        ? HttpTransferState.Cancelled
                        : HttpTransferState.Retrying;
                }
                catch (Exception ex)
                {
                    this.Exception = ex;
                    this.Status = HttpTransferState.Error;
                }
            }
        }
    }
}