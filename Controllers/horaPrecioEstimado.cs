using atiempo_api.Models;
using atiempo_api.App_Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;
using System.Dynamic;

namespace atiempo_api.Controllers
{
    public class horaPrecioEstimado : Controller
    {
        MySqlConnection conn = new MySqlConnection();
        private MainClass appClass = new MainClass();
        CultureInfo ci = new CultureInfo("es-PR");

        //Devolver la hora y el precio estimado
        [HttpPost]
        [Route("conf/horaPrecioEstimado")]
        public ActionResult PostHoraPrecioEstimado([FromBody] PHEstimado data)
        {
            try
            {
                //Recorrer el DataTable para devolver el tiempo y precio estimado de todos los servicios seleccionados.
                //CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                int hora = 0, minutos = 0;
                DateTime timepoServicios = DateTime.Today;
                Double precios = 0;
                string[] time = null;

                foreach (var item in data.estimados)
                {
                    time = item.tiempo.ToString().Split(':');
                    hora = hora + Convert.ToInt32(time[0]);
                    minutos = minutos + Convert.ToInt32(time[1]);
                    precios = precios + Convert.ToDouble(item.precio);
                }

                DateTime tiempoEstimado = timepoServicios.AddMinutes(minutos);
                tiempoEstimado = tiempoEstimado.AddHours(hora);

                var tiempo = tiempoEstimado.ToString("HH:mm");

                var lista = new List<PHEstimado>();
                lista.Add(new PHEstimado()
                {
                    precio = precios.ToString("C", ci),
                    //precio = precios,
                    tiempo = tiempo
                });

                return Ok(lista);
            }
            catch (Exception ex)
            {
                var lista = new List<serverError>();
                lista.Add(new serverError()
                {
                    status = 400,
                    mensaje = "Lo sentimos, ha ocurrido un error.",
                    mensajeServidor = ex.Message + " - " + ex.Data + " - " + ex.HResult + " - " + ex.ToString()
                });
                return Ok(lista);
            }
        }
    }
}

/* using System.Dynamic;

dynamic myObject = new ExpandoObject();

myObject.nombre = "Our Code World";
myObject.sitioweb = "http://ourcodeworld.com";
myObject.lenguaje = "es-CO";

List<string> articulos = new List<string>();
articulos.Add("Como usar JSON con C#");
articulos.Add("Top 5: Mejores calendarios jQuery");
articulos.Add("Otro articulo ... ");

myObject.articulos = articulos;

string json = JsonConvert.SerializeObject(myObject);

Console.WriteLine(json); */

//{  
//   "nombre":"Our Code World",
//   "sitioweb":"http://ourcodeworld.com",
//   "lenguaje":"en-US",
//   "articulos":[
//      "Como usar JSON con C#",
//      "Top 5: Mejores calendarios jQuery",
//      "Otro articulo ... "
//   ]
//}