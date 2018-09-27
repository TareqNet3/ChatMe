using System;
using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ChatMeService.Models
{
    public class ErrorModel
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public string URL { get; set; }
        public string Method { get; set; }
        public object Model { get; set; }

        public ErrorModel InnerError { get; set; }

        public static ErrorModel ProductNotFound(HttpRequest Request)
        {
            return new ErrorModel
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Product Not Found",
                Method = Request.Method,
                URL = Request.Path
            };
        }

        public static ErrorModel BadRequest(HttpRequest Request)
        {
            return new ErrorModel
            {
                StatusCode = HttpStatusCode.BadRequest,
                Method = Request.Method,
                URL = Request.Path,
            };
        }

        public static ErrorModel Forbidden(HttpRequest Request)
        {
            return new ErrorModel
            {
                StatusCode = HttpStatusCode.Forbidden,
                Method = Request.Method,
                URL = Request.Path,
            };
        }

        public static ErrorModel BadRequest(HttpRequest Request, ModelStateDictionary ModelState)
        {
            return new ErrorModel
            {
                StatusCode = HttpStatusCode.BadRequest,
                Method = Request.Method,
                URL = Request.Path,
                Model = ModelState
            };
        }

        public static ErrorModel BadRequestNotEqualsID(HttpRequest Request)
        {
            return new ErrorModel
            {
                StatusCode = HttpStatusCode.BadRequest,
                Method = Request.Method,
                URL = Request.Path,
                Message = "Routed ID is not equals Model ID"
            };
        }
    }
}