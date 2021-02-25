using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace atiempo_api.Models
{
    public class PHEstimado
    {
        public string precio { get; set; }

        public string tiempo { get; set; }
        public IList<estimados> estimados { get; set; }

    }
    public class estimados
    {
        [DataType(DataType.Currency)]
        public double precio { get; set; }
        public string tiempo { get; set; }
    }
}