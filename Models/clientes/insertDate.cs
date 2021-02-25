using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace atiempo_api.Models.clientes
{
    public class insertDate
    {
        [Required]
        public string token { get; set; }
        [Required]
        [Key]
        public string bcaleId { get; set; }
        public string empId { get; set; }
        [Required]
        public string shopId { get; set; }
        [Required]
        public string clientId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string dateSelect { get; set; }
        [Required]
        //[DisplayFormat(DataFormatString = "{HH:mm tt}", NullDisplayText = "Seleccione la hora.")]
        public string hourSelect { get; set; }
        [Required]
        public string tiempoEstimado { get; set; }
        public string clientName { get; set; }
        public string description { get; set; }
        public string payment { get; set; }
        public IList<services> services { get; set; }
    }

    public class services
    {
        [Required]
        public string servId { get; set; }
    }
}