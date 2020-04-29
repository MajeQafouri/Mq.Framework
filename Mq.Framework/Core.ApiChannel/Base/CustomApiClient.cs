using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GajIPG.ApiChannel.Exception;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GajIPG.ApiChannel
{
    public class CustomApiClient : IDisposable
    {
        private HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly TimeSpan _timeout;
        private HttpClientHandler _httpClientHandler;
        private const string ClientUserAgent = "MqApi-v1";
        private const string MediaTypeJson = "application/json";

        public CustomApiClient(string baseUrl, TimeSpan? timeout = null)
        {
            _baseUrl = NormalizeBaseUrl(baseUrl);
            _timeout = timeout ?? TimeSpan.FromSeconds(90);
        }

        public async Task<ApiResult> PostAsync(string url, object input, Dictionary<string, string> headers)
        {
            var result = await PostAsync<string>(url, input, headers);
            result.Content = result.Result;
            return result;
        }

        public async Task<ApiResult<TResult>> PostAsync<TResult>
        (string url, object input, Dictionary<string, string> headers, bool shouldBeSerialized = true)
        {
            try
            {
                PrepareClientRequest(headers);

                using var requestContent = 
                    new StringContent(ConvertToJsonString(input), Encoding.UTF8, MediaTypeJson);
                using var response = await _httpClient.PostAsync(url, requestContent);
                var content = await response.Content.ReadAsStringAsync();
                var apiResult = new ApiResult<TResult>(response.StatusCode, content);

                if (apiResult.IsSuccessStatusCode && shouldBeSerialized)                
                    apiResult.Result = DeSerializeContent<TResult>(content);                    
                
                return apiResult;
            }
            catch (System.Exception ex)
            {
                return new ApiResult<TResult>(HttpStatusCode.InternalServerError, ex?.Message);
            }
        }

        public async Task<ApiResult> GetAsync(string url, Dictionary<string, string> headers)
        {
            var result = await GetAsync<string>(url, headers, false);
            result.Content = result.Result;

            return result;
        }

        public async Task<ApiResult<TResult>> GetAsync<TResult>
            (string url, Dictionary<string, string> headers, bool shouldBeSerialized = true)
        {
            try
            {
                PrepareClientRequest(headers);

                using var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                var apiResult = new ApiResult<TResult>(response.StatusCode, content);

                if (apiResult.IsSuccessStatusCode)                
                    apiResult.Result = DeSerializeContent<TResult>(content);
                
                return apiResult;
            }
            catch (System.Exception ex)
            {
                return new ApiResult<TResult>(HttpStatusCode.InternalServerError, ex?.Message);
            }
        }


        public string CreateQueryString<TObject>(TObject obj, string urn) where TObject : IDto
        {
            var result = string.Join("&", typeof(TObject)
                                       .GetProperties()
                                       .Where(p => Attribute.IsDefined(p, typeof(QueryStringAttribute)) && p.GetValue(obj, null) != null)
                                       .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(obj).ToString())}"));

            result = urn + "?" + result;
            return result;
        }

        public void Dispose()
        {
            _httpClientHandler?.Dispose();
            _httpClient?.Dispose();
        }

        private void PrepareClientRequest(Dictionary<string, string> headers)
        {
            EnsureHttpClientCreated();
            AddHeaders(headers);
        }
        private void CreateHttpClient()
        {
            _httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };

            _httpClient = new HttpClient(_httpClientHandler, false)
            {
                Timeout = _timeout
            };

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ClientUserAgent);

            if (!string.IsNullOrWhiteSpace(_baseUrl))
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
            }

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeJson));
        }

        private void EnsureHttpClientCreated()
        {
            if (_httpClient == null)
            {
                CreateHttpClient();

            }
        }

        private static string ConvertToJsonString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        private TResult DeSerializeContent<TResult>(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<TResult>(content, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        private static string NormalizeBaseUrl(string url)
        {
            return url.EndsWith("/") ? url : url + "/";
        }

        private void AddHeaders(Dictionary<string, string> headers)
        {
            foreach (var header in headers)
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }


    }
}
