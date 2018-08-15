using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace API.Helpers
{
    /// <summary>
    /// Http helper class
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// Default requests timeout
        /// </summary>
        public const int _defaultRequestTimeout = 30000;
        /// <summary>
        /// Default proxy for http requests
        /// </summary>
        public const string _defaultRequestProxy = "";
        /// <summary>
        /// Defautl authorization header for http requests
        /// </summary>
        public const string _defaultRequestAuthorization = "";

        /// <summary>
        /// Create a WebProxy from environment variable, manual input proxy or default server configuration
        /// </summary>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static IWebProxy CreateProxy(string proxy)
        {
            //find proxy on environment variable 
            var envProxy = Environment.GetEnvironmentVariable("proxy", EnvironmentVariableTarget.Process);
            var envProxyUser = Environment.GetEnvironmentVariable("proxyUser", EnvironmentVariableTarget.Process);
            var envProxyPwd =  Environment.GetEnvironmentVariable("proxyPassword", EnvironmentVariableTarget.Process);

            if (proxy == string.Empty && !string.IsNullOrEmpty(envProxy))
                proxy = envProxy;


            if (!string.IsNullOrEmpty(proxy))
            {
                var proxyArgs = proxy.Split(':');
                if (proxyArgs.Length > 1)
                {
                    var tempProxy= new WebProxy(proxyArgs[0], int.Parse(proxyArgs[1]))
                    {
                        UseDefaultCredentials = true
                    };
                    if (!string.IsNullOrEmpty(envProxyUser))
                    {
                        tempProxy.UseDefaultCredentials = false;
                        tempProxy.Credentials = new NetworkCredential(envProxyUser, envProxyPwd);
                    }
                    return tempProxy;
                }
                else
                    return new WebProxy(proxy)
                    {
                        UseDefaultCredentials = true
                    };
            }
            else
            {
                return WebRequest.GetSystemWebProxy();
            }
        }

        /// <summary>
        /// Create a HttpWebRequest
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="timeout"></param>
        /// <param name="contentType"></param>
        /// <param name="proxy"></param>
        /// <param name="authorization"></param>
        /// <param name="otherHeaders"></param>
        /// <returns></returns>
        public static HttpWebRequest CreateHttpRequest(string url, string method, int timeout = _defaultRequestTimeout, string contentType = "", string proxy = _defaultRequestProxy, string authorization = _defaultRequestAuthorization, Dictionary<string, string> otherHeaders = null)
        {
            var httpWebRequest = WebRequest.CreateHttp(url);
            httpWebRequest.Timeout = timeout;
            if (contentType != string.Empty)
                httpWebRequest.ContentType = contentType;
            httpWebRequest.Method = method;
            if(proxy!="-")
                httpWebRequest.Proxy = CreateProxy(proxy);
            if (authorization != string.Empty)
            {
                httpWebRequest.PreAuthenticate = true;
                httpWebRequest.Headers.Add("Authorization", authorization);
            }
            if(otherHeaders != null)
            {
                foreach (var header in otherHeaders)
                {
                    if (header.Value != null)
                        httpWebRequest.Headers.Add(header.Key, header.Value);
                }
            }
            return httpWebRequest;
        }

        /// <summary>
        /// POST sincrona
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonBody"></param>
        /// <param name="result"></param>
        /// <param name="timeout"></param>
        /// <param name="proxy"></param>
        /// <param name="authorization"></param>
        /// <returns></returns>
        public static HttpWebResponse PostJson(string url, string jsonBody, out string result, int timeout = _defaultRequestTimeout, string proxy = _defaultRequestProxy, string authorization = _defaultRequestAuthorization)
        {
            var httpWebRequest = CreateHttpRequest(url, "POST", timeout, "application/json", proxy, authorization);
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonBody);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            return httpResponse;
        }
        /// <summary>
        /// POST async
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonBody"></param>
        /// <param name="timeout"></param>
        /// <param name="proxy"></param>
        /// <param name="authorization"></param>
        /// <returns></returns>
        public static Task<WebResponse> PostJsonAsync(string url, string jsonBody, int timeout = _defaultRequestTimeout, string proxy = _defaultRequestProxy, string authorization = _defaultRequestAuthorization)
        {
            var httpWebRequest = CreateHttpRequest(url, "POST", timeout, "application/json", proxy, authorization);
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonBody);
                streamWriter.Flush();
                streamWriter.Close();
            }

            return httpWebRequest.GetResponseAsync();
        }

        /// <summary>
        /// DELETE async
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonBody"></param>
        /// <param name="timeout"></param>
        /// <param name="proxy"></param>
        /// <param name="authorization"></param>
        /// <returns></returns>
        public static Task<WebResponse> DeleteJsonAsync(string url, string jsonBody, int timeout = _defaultRequestTimeout, string proxy = _defaultRequestProxy, string authorization = _defaultRequestAuthorization)
        {
            var httpWebRequest = CreateHttpRequest(url, "DELETE", timeout, "application/json", proxy, authorization);
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonBody);
                streamWriter.Flush();
                streamWriter.Close();
            }

            return httpWebRequest.GetResponseAsync();
        }


        /// <summary>
        /// POST async
        /// </summary>
        /// <param name="url"></param>
        /// <param name="formData"></param>
        /// <param name="timeout"></param>
        /// <param name="proxy"></param>
        /// <param name="authorization"></param>
        /// <returns></returns>
        public static Task<WebResponse> PostFormAsync(string url, string formData, int timeout = _defaultRequestTimeout, string proxy = _defaultRequestProxy, string authorization = _defaultRequestAuthorization)
        {
            var httpWebRequest = CreateHttpRequest(url, "POST", timeout, "application/x-www-form-urlencoded", proxy, authorization);
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(formData);
                streamWriter.Flush();
                streamWriter.Close();
            }

            return httpWebRequest.GetResponseAsync();
        }
        /// <summary>
        /// PUT async
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonBody"></param>
        /// <param name="timeout"></param>
        /// <param name="proxy"></param>
        /// <param name="authorization"></param>
        /// <returns></returns>
        public static Task<WebResponse> PutJsonAsync(string url, string jsonBody, int timeout = 60000, string proxy = _defaultRequestProxy, string authorization = _defaultRequestAuthorization)
        {
            var httpWebRequest = CreateHttpRequest(url, "PUT", timeout, "application/json", proxy, authorization);
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonBody);
                streamWriter.Flush();
                streamWriter.Close();
            }

            return httpWebRequest.GetResponseAsync();
        }

        /// <summary>
        /// GET sync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="result"></param>
        /// <param name="timeout"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static HttpWebResponse GetString(string url, out string result, int timeout = _defaultRequestTimeout, string proxy = _defaultRequestProxy)
        {
            var httpWebRequest = CreateHttpRequest(url, "GET", timeout, string.Empty, proxy);
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            return httpResponse;
        }

        /// <summary>
        /// GET async
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout"></param>
        /// <param name="proxy"></param>
        /// <param name="authorization"></param>
        /// <param name="otherHeaders"></param>
        /// <returns></returns>
        public static Task<WebResponse> GetStringAsync(string url, int timeout = _defaultRequestTimeout, string proxy = _defaultRequestProxy, string authorization = _defaultRequestAuthorization, Dictionary<string, string> otherHeaders = null)
        {
            return CreateHttpRequest(url, "GET", timeout, string.Empty, proxy, authorization, otherHeaders).GetResponseAsync();
        }

        /// <summary>
        /// Generic GET async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="timeOut"></param>
        /// <param name="proxy"></param>
        /// <param name="authorization"></param>
        /// <param name="otherHeaders"></param>
        /// <returns></returns>
        public static async Task<T> GetStringAsync<T>(string url, int timeOut = _defaultRequestTimeout, string proxy = _defaultRequestProxy, string authorization = _defaultRequestAuthorization, Dictionary<string, string> otherHeaders = null) where T : class
        {
            var httpResponse = await GetStringAsync(url, timeOut, proxy, authorization, otherHeaders) as HttpWebResponse;
            // TODO: Handle more explicitly the different status codes, especially the ones associated to authorization
            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var json = await streamReader.ReadToEndAsync();
                    if (json != null)
                    {
                        return JsonConvert.DeserializeObject<T>(json);
                    }
                }
            }
            return null;
        }
    }
}