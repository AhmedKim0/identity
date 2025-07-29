using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DTO
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public List<Error>? errors {get; set; } = new List<Error>();

        public static Response<T> SuccessResponse(T data)
        {
                return new Response<T>
                {
                    Success = true,
                    Data = data
                };
        }

        public static Response<T> Failure(List<Error> errors)
        {
            return new Response<T>
            {
                Success = false,
                Data = default,
                errors= errors
            };
        }
        public static Response<T> Failure( Error error)
        {

            return new Response<T>
            {
                Success = false,
                Data = default,
                errors = new List<Error> { error }
            };
        }


    }
    public class Error
    {
        public Error(string message, string? code = null)
        {
            Message = message;
            Code = code;
        }
        public string Message { get; set; }
        public string? Code { get; set; }
    }
}


