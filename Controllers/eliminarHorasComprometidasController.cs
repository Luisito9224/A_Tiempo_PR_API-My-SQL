using atiempo_api.Models;
using atiempo_api.App_Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Dynamic;

namespace atiempo_api.Controllers
{
    public class eliminarHorasComprometidasController : Controller
    {
        MySqlConnection conn = new MySqlConnection();
        private MainClass appClass = new MainClass();
        CultureInfo ci = new CultureInfo("es-PR");

        //Se deben enviar las horas una a una desde el app que consuma esta API
        [HttpPost]
        [Route("conf/eliminarHorasComprometidas")]
        public ActionResult postValidarHora([FromBody] eliminarHoras data)
        {
            TimeZoneInfo puertoRico = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
            DateTime diaActual = TimeZoneInfo.ConvertTime(DateTime.Now, puertoRico);
            var serverMessage = new List<serverError>();
            dynamic myObject = new ExpandoObject();
            List<string> horasDisponibles = new List<string>();
            var lista = new List<horasDisponibles>();

            try
            {
                if (ModelState.IsValid)
                {
                    foreach (var item in data.hour)
                    {
                        string diaHoraActualMilitar = diaActual.ToString("MM/dd/yyyy HH:mm").Replace(".", "/");
                        string diahoraSelectMilitar = Convert.ToDateTime(data.date + " " + item.horas).ToString("MM/dd/yyyy HH:mm").Replace(".", "/");

                        DateTime date = Convert.ToDateTime(diaHoraActualMilitar, ci.DateTimeFormat);
                        DateTime date2 = Convert.ToDateTime(diahoraSelectMilitar, ci.DateTimeFormat);

                        string diaSemanaSelect = date2.ToString("dddd");

                        string horaValida = appClass.ocultarhorasComprometidas(data.date, item.horas, data.shopId, data.empId, data.timeEstimated, date, date2);
                        if (horaValida == "Y")
                        {
                            //horasDisponibles.Add(item.horas);
                            lista.Add(new horasDisponibles()
                            {
                                hora = item.horas
                            });
                        }
                    }
                    //myObject.hora = horasDisponibles;
                    //string json = JsonConvert.SerializeObject(myObject);
                    return Ok(lista);
                }
                else
                {
                    //return Ok(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                serverMessage.Add(new serverError()
                {
                    status = 400,
                    mensaje = "Lo sentimos, ha ocurrido un error.",
                    mensajeServidor = ex.Message + " - " + ex.Source + " - " + ex.InnerException + " - " + ex.ToString()
                });
                return Ok(serverMessage);
            }
        }

    }
}
