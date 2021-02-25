using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace atiempo_api.Models.clientes
{
    public class locales
    {
        [Key]
        public string id { get; set; }

        public string name { get; set; }
        public string banner { get; set; }
        public string description { get; set; }

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [StringLength(100, ErrorMessage = "Logitud máxima 100")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email incorrecto")]
        [EmailAddress(ErrorMessage = "Correo electrónico incorrecto")]
        public string email { get; set; }
        public string phone { get; set; }
        public string faceBook { get; set; }
        public string instagram { get; set; }
        public string googleMaps { get; set; }
    }
}