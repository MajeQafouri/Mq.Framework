using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GajIPG.ApiChannel
{
    public class ApiResult : IApiResult
    {
        public ApiResult()
        {

        }
        public ApiResult(HttpStatusCode statusCode, string content)
        {
            ResponseStatusCode = statusCode;
            Content = content;
        }
        public bool IsSuccessStatusCode
        {
            get { return ResponseStatusCode == HttpStatusCode.OK; }
        }

        public HttpStatusCode ResponseStatusCode { get; set; }
        public string Content { get; set; }
    }
    public class ApiResult<TResult> : ApiResult
    {
        public ApiResult(HttpStatusCode statusCode, string content) :base(statusCode, content)
        {

        }
        public TResult Result { get; set; }
    }
}
