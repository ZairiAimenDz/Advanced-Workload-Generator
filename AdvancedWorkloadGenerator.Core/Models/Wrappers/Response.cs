using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedWorkloadGenerator.Core.Models.Wrappers
{
    public class Response<T>
    {
        public Response()
        {
            Errors = new List<string>();
        }

        public bool IsSuccess { get; set; } = true;
        public T? Data { get; set; }
        public List<string> Errors { get; set; }

        public static Response<T> Success(T data)
        {
            return new Response<T> { Data = data, IsSuccess = true };
        }
        public static Response<T> Failure(List<string> errors)
        {
            return new Response<T> { IsSuccess = false, Errors = errors };
        }
        public static Response<T> Failure(string error)
        {
            return new Response<T> { IsSuccess = false, Errors = new List<string> { error } };
        }
    }
}
