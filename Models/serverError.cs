using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace atiempo_api.Models
{
    public class serverError
    {
        public int status { get; set; }
        public string mensaje { get; set; }
        public string mensajeServidor { get; set; }
    }
}