using MemAnalyze.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MemAnalyze.ViewModels
{
    public class MemberView
    {
        public string Account { get; set; }
        public string CdName { get; set; }
        public string Password { get; set; }
        public string OdPassword { get; set; }
        //public string Name { get; set; }
        //public string Address { get; set; }
        //public DateTime Birthday { get; set; }
        //public string Job { get; set; }
        //public string Gender { get; set; }
        //public string Imageurl { get; set; }
        public string result { get; set; }
    }
}