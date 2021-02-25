using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace atiempo_api.Models
{
    public class citasCalendario
    {
        public string token { get; set; }
        public string idCliente { get; set; }
        public string idCompania { get; set; }
        public string idEmpleado { get; set; }
        public string idCalendario { get; set; }
        public string nombreEmpleado { get; set; }
        public string nombreLocal { get; set; }
        public string fechaComienza { get; set; }
        public string fechaTermina { get; set; }
        public string hora { get; set; }
        public string tracking { get; set; }
        public string recibo { get; set; }
        public IList<serviciosCitas> servicios { get; set; }
    }
    public class serviciosCitas
    {
        public string id { get; set; }
        public string services { get; set; }
        public string time { get; set; }
        public string price { get; set; }
        public string description { get; set; }
    }
}