using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace atiempo_api.Models
{
    public class enviarMensaje
    {
        [Required(ErrorMessage = "El id es requerido.")]
        public string id { get; set; }
        [StringLength(50, ErrorMessage = "El máximo de caracteres es de 50."), Required(ErrorMessage = "El asunto es requerido.")]
        public string asunto { get; set; }
        [Required(ErrorMessage = "El mensaje es requerido.")]
        public string mensaje { get; set; }
        [Required(ErrorMessage = "La hora es requerida.")]
        public string email { get; set; }
        public string phone { get; set; }
    }
}