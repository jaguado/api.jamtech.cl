using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JAMTech.Helpers
{
    /// <summary>
    /// Provides access to network tools
    /// </summary>
    public static class Net
    {

        /// <summary>
        /// Gets the number of milliseconds taken to send an Internet Control Message Protocol
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="timeout"></param>
        /// <param name="ttl"></param>
        /// <returns></returns>
        public static async Task<long> Ping(string hostname, int timeout=12000, int ttl=64)
        {
             var pingSender = new Ping();
            // Create a buffer of 32 bytes of data to be transmitted.  
            var data = new String('a', 32);
            var buffer = Encoding.ASCII.GetBytes(data);
  
            var options = new PingOptions(ttl, true);
            var reply = await pingSender.SendPingAsync(hostname, timeout, buffer, options);
            if (reply == null || reply.Status!=IPStatus.Success) return -1;
            return reply.RoundtripTime;
        }

        /// <summary>
        /// Check if can connect to the specified TCP port on the specified host as an asynchronous operation.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<long> Telnet(string hostname, int port, int timeout = 3000)
        {
            return await Task.Run(() =>
            {
                using (var tcp = new TcpClient())
                {
                    using (var ct = new CancellationTokenSource(timeout))
                    {
                        try
                        {
                            var timer = Stopwatch.StartNew();
                            var connectTask = tcp.ConnectAsync(hostname, port);
                            Task.WaitAll(new[] { connectTask }, ct.Token);
                            timer.Stop();
                            return connectTask.IsCompletedSuccessfully && tcp.Connected ? timer.ElapsedMilliseconds : -1;
                        }
                        catch (Exception)
                        {
                            return -1;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Generic http request operation similar to curl
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="payload"></param>
        /// <param name="contentType"></param>
        /// <param name="timeout"></param>
        /// <param name="useProxy"></param>
        /// <returns></returns>
        public static async Task<string> Curl(string url,  string method, string payload, string contentType, int timeout=30000, bool useProxy=false)
        {
            var request = WebRequest.CreateHttp(url);
            if (useProxy)
                request.Proxy = Http.CreateProxy("");
            request.Timeout = timeout;
            request.Method = method;
            request.ContentType = contentType;
            if (method != "GET" && !string.IsNullOrEmpty(payload))
            {
                var requestStream = await request.GetRequestStreamAsync();
                var streamWriter = new StreamWriter(requestStream);
                await streamWriter.WriteAsync(payload);
            }
            var response = await request.GetResponseAsync();
            var stream = new StreamReader(response.GetResponseStream());
            return await stream.ReadToEndAsync();
        }

        public static async Task<HttpResponseMessage> GetResponse(string tempUrl, Uri customReferrer = null, int timeoutInSeconds=0)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate                
            };
            var client = new HttpClient(handler);
            if(timeoutInSeconds>0)
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            if (customReferrer != null)
                client.DefaultRequestHeaders.Referrer = customReferrer;
            return await client.GetAsync(tempUrl);
        }
        public static async Task<HttpResponseMessage> PostResponse(string tempUrl, HttpContent content, Uri customReferrer = null, int timeoutInSeconds = 0)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var client = new HttpClient(handler);
            if (timeoutInSeconds > 0)
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            if (customReferrer != null)
                client.DefaultRequestHeaders.Referrer = customReferrer;
            return await client.PostAsync(tempUrl, content);
        }
        public static async Task<HttpResponseMessage> PutResponse(string tempUrl, HttpContent content, Uri customReferrer = null, int timeoutInSeconds = 0)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var client = new HttpClient(handler);
            if (timeoutInSeconds > 0)
                client.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            if (customReferrer != null)
                client.DefaultRequestHeaders.Referrer = customReferrer;
            return await client.PutAsync(tempUrl, content);
        }

        ///
        /// Checks the file exists or not.
        ///
        /// The URL of the remote file.
        /// True : If the file exits, False if file not exists
        public static async Task<bool> RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                var request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    return (response.StatusCode == HttpStatusCode.OK);
                }
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

        ///
        /// Checks the file exists or not.
        ///
        /// The URL of the remote file.
        /// True : If the file exits, False if file not exists
        public static async Task<bool> RemoteUrlExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                var request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "GET";
                //Getting the Web Response.
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    return (response.StatusCode == HttpStatusCode.OK);
                }
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

    }
}
