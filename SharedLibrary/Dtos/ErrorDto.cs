using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary.Dtos
{
   public class ErrorDto
    {
        public List<string> Errors { get; private set; } = new List<string>();
        public bool IsShow { get; private set; } // true ise kullanıcıya gösterilebilir


        //public ErrorDto()
        //{
        //    Errors = new List<string>();
        //}
        public ErrorDto(string error, bool isShow)
        {
            Errors.Add(error);
            IsShow = isShow;
        }
        public ErrorDto(List<string>errors, bool isShow)
        {
            Errors = errors;
            IsShow = isShow;
        }
    }
}
