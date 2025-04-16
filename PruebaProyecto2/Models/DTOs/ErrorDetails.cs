using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GYMISFAMILY.Models.DTOs
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

    }

}