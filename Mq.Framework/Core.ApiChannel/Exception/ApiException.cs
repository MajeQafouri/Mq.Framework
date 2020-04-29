using System.Net;

namespace GajIPG.ApiChannel.Exception
{
    public class ApiException :  System.Exception
    {
        public ApiException(HttpStatusCode statusCode, string content)
        {
            StatusCode = (int)statusCode;
            Content = content;
        }
        public int StatusCode { get; set; }
        public string Content { get; set; }
    }
}
