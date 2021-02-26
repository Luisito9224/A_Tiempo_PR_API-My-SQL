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
using System.Data;
using System.Globalization;

namespace atiempo_api.Controllers
{
    public class dropDowns : Controller
    {
        MySqlConnection conn = new MySqlConnection();
        private MainClass appClass = new MainClass();
        CultureInfo ci = new CultureInfo("es-PR");

        //Se deben enviar las horas una a una desde el app que consuma esta API  Services: atiempoPR_API
        [HttpGet]
        [Route("conf/careers")]
        public ActionResult getCareers()
        {
            var serverMessage = new List<serverError>();
            var lista = new List<careers>();

            try
            {
                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"SELECT MCAREERS_ID AS ID, MCAREERS_VALUE AS DATA_VALUE, MCAREERS_COMPANY_NAME AS COMPANY_NAME FROM MCAREERS WHERE MCAREERS_STATUS = 'A' ORDER BY COMPANY_NAME");
                conn.Open();
                cmd.Connection = conn;
                MySqlDataReader rdr = cmd.ExecuteReader();
                //rdr.Read();
                lista.Add(new careers()
                {
                    id = "0",
                    companias = "Seleccione",
                    value = "0"
                });
                while (rdr.Read())
                {
                    lista.Add(new careers()
                    {
                        id = rdr["ID"].ToString(),
                        companias = rdr["COMPANY_NAME"].ToString(),
                        value = rdr["DATA_VALUE"].ToString()
                    });
                }
                return Ok(lista);
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
