using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace atiempo_api.Models.clientes
{
    public class client
    {

        public string token { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string lastName { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string Careers { get; set; }
    }
}