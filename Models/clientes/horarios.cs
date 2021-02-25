using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace atiempo_api.Models.clientes
{
    public class horarios
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string Open { get; set; }
        public string Close { get; set; }
        public string DateActualOpenOrClose { get; set; }
    }
}