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
using System.Dynamic;

namespace atiempo_api.Controllers
{
    public class enviarRecordatorios : Controller
    {
        MySqlConnection conn = new MySqlConnection();
        private MainClass appClass = new MainClass();
        CultureInfo ci = new CultureInfo("es-PR");

        //Se deben enviar las horas una a una desde el app que consuma esta API
        [HttpGet]
        [Route("conf/enviarNotificaciones")]
        public ActionResult getCareers()
        {
            var serverMessage = new List<serverError>();
            var lista = new List<careers>();

            try
            {
                string query = @"SELECT FORMAT(CONVERT(datetime, BCALE_START), 'hh:mm tt') AS START_H,
                                    FORMAT(CONVERT(datetime, BCALE_START), 'MM/dd/yyyy') AS FECHA_START,
	                                CONCAT(BEMPL_NAME, ' ', BEMPL_LAST_NAME) AS EMPLOYE_NAME,
	                                CASE WHEN BCLIE_NAME IS NOT NULL  THEN CONCAT(BCLIE_NAME, ' ', BCLIE_LAST_NAME) ELSE REPLACE(BCALE_TITLE, 'Cita con: ', '') END AS CLIENT_NAME,
	                                BCLIE_EMAIL AS CLIENT_EMAIL,
	                                (SELECT BCLIE_PHONE + MCAREERS_VALUE FROM MCAREERS WHERE MCAREERS_ID = BCLIE_CAREERS) AS TEST_MESSAGE,
	                                BSHOP_NAME AS SHOP_NAME,
                                    BCLIE_ID AS CLIENT_ID,
                                    BCALE_ID AS CALE_ID
                               FROM BCALE, BCLIE, BEMPL, BSHOP
                              WHERE BCALE_END >= GETDATE() AND BCALE_END <= GETDATE() + 1
                                AND BCALE_NOTIFY = 0
                                AND BCALE_BSHOP_ID = BSHOP_ID
                                AND BCALE_BEMPL_ID = BEMPL_ID
                                AND BCALE_CLIENT_ID = BCLIE_ID
                                AND BCALE_CLIENT_ID <> '1'
                                AND BCALE_STATUS = 'A'
                              ORDER BY BCALE_END DESC";

                MySqlCommand cmd = new MySqlCommand(query);
                if (conn.State == ConnectionState.Closed)
                {
                    conn.ConnectionString = appClass.GetConn();
                    conn.Open();
                }
                cmd.Connection = conn;
                MySqlDataReader rdr = cmd.ExecuteReader();
                dynamic result = new ExpandoObject();

                while (rdr.Read())
                {
                    string emailCliente = rdr["CLIENT_EMAIL"].ToString();
                    string mensajeTextoCliente = rdr["TEST_MESSAGE"].ToString();
                    string nombreCliente = rdr["CLIENT_NAME"].ToString();
                    string nombreEmpleado = rdr["EMPLOYE_NAME"].ToString();
                    string nombreLocal = rdr["SHOP_NAME"].ToString();
                    string clientID = rdr["CLIENT_ID"].ToString();
                    string CitaID = rdr["CALE_ID"].ToString();

                    string validar = appClass.ejecutarScript(@"UPDATE BCALE SET BCALE_NOTIFY = 1, BCALE_ACTIVITY_DATE =  SYSDATETIME() WHERE BCALE_CLIENT_ID = '" + clientID + "' AND BCALE_ID = '" + CitaID + "'");
                    if (validar == "")
                    {
                        //Enviar email al cliente.
                        string mensaje = @"Saludos,<br/><br/>Deseamos recordarle que tiene una cita el " + rdr["FECHA_START"].ToString() + " a las " + rdr["START_H"].ToString() + " con " + nombreEmpleado + ". Le estaremos esperando.";
                        string send = appClass.sendEmail(emailCliente, mensaje, "Recordatorio de cita");

                        //Enviar mensaje de texto al empleado
                        if (mensajeTextoCliente != "")
                            appClass.sendText(mensajeTextoCliente, "Saludos, deseamos recordarle que tiene una cita el " + rdr["FECHA_START"].ToString() + " a las " + rdr["START_H"].ToString() + " con " + nombreEmpleado + ". Le estaremos esperando.", "Recordatorio de cita");
                    }
                }
                rdr.Close();

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