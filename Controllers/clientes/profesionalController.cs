using atiempo_api.Models.clientes;
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

namespace atiempo_api.Controllers.clientes
{
    public class profesionalController : Controller
    {
        MySqlConnection conn = new MySqlConnection();
        private MainClass appClass = new MainClass();
        CultureInfo ci = new CultureInfo("es-PR");
        MySqlTransaction transaction;

        //Servicios de los empleados
        [HttpGet]
        [Route("clie/servicesEmp")]
        public ActionResult getAllLocal(string empId, string shopId)
        {
            //[FromBody] string token
            try
            {
                //if (!appClass.validateToken(token))
                //return Ok("{'token': 'Token not valid'}");
                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"SELECT BSERV_ID AS ID,
                                                         BSERV_NAME AS SERVICIO, 
                                                         BSERV_PRICE AS PRICE, 
                                                         BSERV_TIME AS TIME,
                                                         BSERV_DESCRIPTION AS DESCRIPTION
                                                    FROM BSERV, RSERV 
                                                   WHERE RSERV_BEMPL_ID = @EMP_ID
                                                     AND BSERV_BSHOP_ID = @SHOP_ID
                                                     AND BSERV_STATUS = 'A'
                                                     AND RSERV_BSERV_ID = BSERV_ID 
                                                   ORDER BY BSERV_SORT, BSERV_NAME");
                conn.Open();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@EMP_ID", empId);
                cmd.Parameters.AddWithValue("@SHOP_ID", shopId);
                MySqlDataReader rdr = cmd.ExecuteReader();
                //rdr.Read();

                var lista = new List<servicios>();
                while (rdr.Read())
                {
                    lista.Add(new servicios()
                    {
                        id = rdr["ID"].ToString(),
                        services = rdr["SERVICIO"].ToString(),
                        price = rdr["PRICE"].ToString(),
                        time = rdr["TIME"].ToString(),
                        description = rdr["DESCRIPTION"].ToString()
                    });
                }

                return Ok(lista);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        //Crear una cita con un empleado
        [HttpPost]
        [Route("clie/createDate")]
        public ActionResult postCreateDate([FromBody] insertDate data)
        {
            DateTime date = Convert.ToDateTime(data.dateSelect);
            string diaSelect = date.ToString("yyyy-MM-dd", ci);
            Guid BCALE_ID = Guid.NewGuid();

            var serverMessage = new List<serverError>();
            try
            {
                if (appClass.validateToken(data.token) == false)
                {
                    serverMessage.Add(new serverError()
                    {
                        status = 100,
                        mensaje = "La sesión ha expirado.",
                        mensajeServidor = ""
                    });
                    return Ok(serverMessage);
                }

                DateTime diaActual = DateTime.Now;
                string dia = date.ToString("dddd", ci);
                string fechaActual = diaActual.ToString("MM/dd/yyyy");

                bool diaDeTrabajo = appClass.existe(@"SELECT BEHOU_DATE FROM BEHOU WHERE UPPER(BEHOU_DATE) = UPPER('" + dia + "') AND BEHOU_STATUS = 'A' AND BEHOU_BEMPL_ID = '" + data.empId + "'");
                string diaDeVacaciones = appClass.retornarValor(@"SELECT BHOLI_DESCRIPTION FROM BHOLI WHERE '" + diaSelect + "' BETWEEN BHOLI_START AND BHOLI_END AND BHOLI_STATUS = 'A' AND BHOLI_BEMPL_ID = '" + data.empId + "'");

                if (diaActual > date.AddDays(1))
                {
                    serverMessage.Add(new serverError()
                    {
                        status = 300,
                        mensaje = "No puede coordinar una cita en dias pasados.",
                        mensajeServidor = ""
                    });
                    return Ok(serverMessage);
                }
                else if (diaDeTrabajo == false)
                {
                    if (data.dateSelect == diaActual.ToString("MM/dd/yyyy"))
                    {
                        serverMessage.Add(new serverError()
                        {
                            status = 300,
                            mensaje = "El empleado no trabaja hoy. Por favor seleccione otro día.",
                            mensajeServidor = ""
                        });
                    }
                    else
                    {
                        serverMessage.Add(new serverError()
                        {
                            status = 300,
                            mensaje = "El empleado no trabaja el día " + dia,
                            mensajeServidor = ""
                        });
                    }

                    return Ok(serverMessage);
                }
                else if (diaDeVacaciones != "")
                {
                    serverMessage.Add(new serverError()
                    {
                        status = 300,
                        mensaje = "El empleado se encuentra de vacaciones.",
                        mensajeServidor = ""
                    });
                    return Ok(serverMessage);
                }
                else
                {
                    //Validar que no exista una cita para la hora seleccionada.
                    bool noExistecita = appClass.existe(@"SELECT 'X' FROM BCALE WHERE BCALE_BSHOP_ID = '" + data.shopId + "' AND BCALE_BEMPL_ID = '" + data.empId + "' AND BCALE_START = '" + diaSelect + " " + data.hourSelect + "' AND BCALE_STATUS = 'A'");
                    if (noExistecita == true)
                    {
                        serverMessage.Add(new serverError()
                        {
                            status = 300,
                            mensaje = "Lo sentimos, ya se agendó una cita para la hora " + data.hourSelect,
                            mensajeServidor = ""
                        });
                        return Ok(serverMessage);
                    }

                    string clientName = appClass.retornarValor(@"SELECT BCLIE_NAME FROM BCLIE WHERE BCLIE_ID = '" + data.clientId + "'");

                    //Guardar la cita
                    MySqlCommand cmd = new MySqlCommand(@"INSERT INTO BCALE (BCALE_ID, 
                                                                BCALE_BSHOP_ID, 
                                                                BCALE_CLIENT_ID, 
                                                                BCALE_BEMPL_ID, 
                                                                BCALE_TITLE, 
                                                                BCALE_DESCRIPTION,
                                                                BCALE_DATE,
                                                                BCALE_START, 
                                                                BCALE_END, 
                                                                BCALE_TRACKING,
                                                                BCALE_NOTIFY, 
                                                                BCALE_CLASSNAME, 
                                                                BCALE_ALL_DAY, 
                                                                BCALE_STATUS, 
                                                                BCALE_ACTIVITY_DATE, 
                                                                BCALE_USER_ID)
                                                         VALUES (@BCALE_ID, 
                                                                @BCALE_BSHOP_ID, 
                                                                @BCALE_CLIENT_ID, 
                                                                @BCALE_BEMPL_ID, 
                                                                @BCALE_TITLE, 
                                                                @BCALE_DESCRIPTION, 
                                                                @DATE_SELECT,
                                                                @BCALE_START, 
                                                                @BCALE_END, 
                                                                @CONFIRM,
                                                                0,
                                                                'fc-warning',
                                                                0,
                                                                'A', 
                                                                SYSDATETIME(),
                                                                @USER_ID)");
                    conn.ConnectionString = appClass.GetConn();
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    cmd.Transaction = transaction;
                    cmd.Connection = conn;
                    cmd.Parameters.AddWithValue("@BCALE_ID", BCALE_ID);
                    cmd.Parameters.AddWithValue("@BCALE_BSHOP_ID", data.shopId);
                    cmd.Parameters.AddWithValue("@BCALE_CLIENT_ID", data.clientId);
                    cmd.Parameters.AddWithValue("@BCALE_BEMPL_ID", data.empId);

                    cmd.Parameters.AddWithValue("@BCALE_TITLE", "Cita con: " + clientName);

                    if (data.description == "")
                        cmd.Parameters.AddWithValue("@BCALE_DESCRIPTION", "Ha programado una cita con usted.");
                    else
                        cmd.Parameters.AddWithValue("@BCALE_DESCRIPTION", data.description);
                    cmd.Parameters.AddWithValue("@DATE_SELECT", diaSelect);
                    //cmd.Parameters.AddWithValue("@BCALE_START", diaSelect + " " + data.hourSelect);
                    cmd.Parameters.Add("@BCALE_START", MySqlDbType.DateTime).Value = diaSelect + " " + data.hourSelect;

                    string[] HM = data.tiempoEstimado.Split(':');
                    DateTime horaTermina = Convert.ToDateTime(data.hourSelect).AddHours(Convert.ToInt32(HM[0])).AddMinutes(Convert.ToInt32(HM[1]));
                    //cmd.Parameters.AddWithValue("@BCALE_END", diaSelect + " " + horaTermina.ToString("HH:mm tt"));

                    cmd.Parameters.Add("@BCALE_END", MySqlDbType.DateTime).Value = diaSelect + " " + horaTermina.ToString("HH:mm tt");

                    //Si realiza un pago.
                    if (data.payment == "")
                        cmd.Parameters.AddWithValue("@CONFIRM", "P");
                    else
                        cmd.Parameters.AddWithValue("@CONFIRM", "C");

                    cmd.Parameters.AddWithValue("@USER_ID", data.clientId);
                    cmd.ExecuteNonQuery();

                    //Insertar los servicios
                    MySqlCommand cmd2 = new MySqlCommand(@"INSERT INTO RSCAL(RSCAL_BCALE_ID, RSCAL_RSERV_ID, RSCAL_ACTIVITY_DATE) VALUES(@BCALE_ID, @RSERV_ID, SYSDATETIME())", conn);
                    cmd2.Transaction = transaction;
                    foreach (var item in data.services)
                    {
                        cmd2.Parameters.Clear();
                        cmd2.Parameters.AddWithValue("@BCALE_ID", BCALE_ID);
                        cmd2.Parameters.AddWithValue("@RSERV_ID", item.servId);
                        cmd2.ExecuteNonQuery();
                    }

                    //Enviar las notificaciones
                    string emailEmpleado = appClass.retornarValor(@"SELECT BEMPL_EMAIL FROM BEMPL WHERE BEMPL_ID = '" + data.empId + "'");
                    string empPhone = appClass.retornarValor(@"SELECT CONCAT(BEMPL_PHONE, MCAREERS_VALUE) FROM BEMPL, MCAREERS WHERE BEMPL_CAREERS = MCAREERS_ID AND BEMPL_ID = '" + data.empId + "'");

                    //appClass.sendText(empPhone, "Saludos, " + data.clientName + " ha programada una cita con usted para la fecha " + diaSelect + " a la hora " + data.hourSelect + ".", "Cita programada");

                    transaction.Commit();

                    serverMessage.Add(new serverError()
                    {
                        status = 200,
                        mensaje = "La cita ha sido programada para la fecha " + diaSelect + " a la hora " + data.hourSelect + ". Pronto recibira un mensaje con la confirmación.",
                        mensajeServidor = ""
                    });
                    return Ok(serverMessage);
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                transaction.Rollback();

                string[] HM = data.tiempoEstimado.Split(':');
                DateTime horaTermina = Convert.ToDateTime(data.hourSelect).AddHours(Convert.ToInt32(HM[0])).AddMinutes(Convert.ToInt32(HM[1]));

                serverMessage.Add(new serverError()
                {
                    status = 400,
                    mensaje = "Lo sentimos, ha ocurrido un error.",
                    mensajeServidor = "Día: " + diaSelect + " Hora: " + data.hourSelect + " Hora en que termina:" + horaTermina.ToString("HH:mm tt") + " | " + ex.Message
                });
                return Ok(serverMessage);
            }
        }
    }
}
