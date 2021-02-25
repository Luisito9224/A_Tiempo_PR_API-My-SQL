using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace atiempo_api.Models
{
    public class eliminarHoras
    {
        [StringLength(10, ErrorMessage = "El máximo de caracteres es de 10."), Required(ErrorMessage = "La fecha es requerida.")]
        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string date { get; set; }
        [Required(ErrorMessage = "El local es requerido.")]
        public string shopId { get; set; }
        [Required(ErrorMessage = "El empleado es requerido.")]
        public string empId { get; set; }
        [Required(ErrorMessage = "El tiempo estimado es requerido.")]
        //public System.TimeSpan timeEstimated { get; set; }
        public string timeEstimated { get; set; }
        [Required(ErrorMessage = "La hora es requerida.")]
        public IList<hour> hour { get; set; }

    }
    public class hour
    {
        [Required]
        public string horas { get; set; }
    }

    public class horasDisponibles
    {
        public string hora { get; set; }
    }
}