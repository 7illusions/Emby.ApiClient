using Emby.ApiInteraction.Net;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Emby.ApiInteraction
{
    public class PortableHttpWebRequestFactory : IHttpWebRequestFactory
    {
        public HttpWebRequest Create(HttpRequest options)
        {
            var request = HttpWebRequest.CreateHttp(options.Url);

            request.Method = options.Method;

            return request;
        }

        public void SetContentLength(HttpWebRequest request, long length)
        {
            //request.Headers["Content-Length"] = length.ToString(CultureInfo.InvariantCulture);
        }

        public Task<WebResponse> GetResponseAsync(HttpWebRequest request, int timeoutMs)
        {
            var tcs = new TaskCompletionSource<WebResponse>();
            if (timeoutMs > 0)
            {
                var ct = new CancellationTokenSource(timeoutMs);
                ct.Token.Register(() =>
                {
                    tcs.TrySetException(new TimeoutException());
                }, useSynchronizationContext: false);

            }

            try
            {
                request.BeginGetResponse(iar =>
                {
                    try
                    {
                        var response = (HttpWebResponse)request.EndGetResponse(iar);                        
                        if (!tcs.Task.IsCanceled)
                            tcs.SetResult(response);
                    }
                    catch (Exception exc)
                    {
                        tcs.TrySetException(exc);
                    }
                }, null);
            }
            catch (Exception exc)
            {
                tcs.TrySetException(exc);
            }

            return tcs.Task;
        }

        //private Task<WebResponse> GetResponseAsync(WebRequest request, TimeSpan timeout)
        //{
        //    return Task.Factory.StartNew(() =>
        //    {
        //        var t = Task.Factory.FromAsync(
        //            request.BeginGetResponse,
        //            request.EndGetResponse,
        //            null);

        //        try
        //        {
        //            if (!t.Wait(timeout))
        //            {
        //                throw new TimeoutException();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            throw e.InnerException ?? e;
        //        }

        //        return t.Result;
        //    });
        //}

        public Task<Stream> GetRequestStreamAsync(HttpWebRequest request)
        {
            return request.GetRequestStreamAsync();
        }
    }
}
