using atiempo_api.Models.clientes;
using atiempo_api.App_Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using atiempo_api.Models;

namespace atiempo_api.Controllers
{
    public class sendMessage : Controller
    {
        MySqlConnection conn = new MySqlConnection();
        private MainClass appClass = new MainClass();
        CultureInfo ci = new CultureInfo("es-PR");

        //Se deben enviar las horas una a una desde el app que consuma esta API
        [HttpGet]
        [Route("conf/enviarMensaje")]
        public ActionResult GetSendMessage([FromBody] enviarMensaje data)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string horaValida = appClass.sendText(data.id, data.asunto, data.mensaje);
                    return Ok(horaValida);
                }

                //return Ok(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
                return BadRequest(ModelState);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

    }
}
