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
using System.Dynamic;

namespace atiempo_api.Controllers.clientes
{
    public class clientController : Controller
    {
        MySqlConnection conn = new MySqlConnection();
        private MainClass appClass = new MainClass();

        //Login del cliente
        [HttpPost]
        [Route("clie/login")]
        public ActionResult loginClient([FromBody] client data)
        {
            var serverMessage = new List<serverError>();
            try
            {
                string phone = data.phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Trim();

                string validarPhone = appClass.retornarValor(@"SELECT 'X' FROM BCLIE 
                                                                WHERE UPPER(BCLIE_PHONE) = UPPER('" + phone + @"')
                                                                  AND BCLIE_STATUS = 'I'");
                if (validarPhone == "X")
                {
                    serverMessage.Add(new serverError()
                    {
                        status = 300,
                        mensaje = "Debe confirmar su cuenta. ¿Desea que le reenviemos el mensaje de confirmación?"
                    });
                    return Ok(serverMessage);
                }

                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"SELECT BCLIE_ID AS BCLIE_ID, 
                                                         BCLIE_NAME AS NAME, 
                                                         BCLIE_LAST_NAME AS LAST_NAME, 
                                                         BCLIE_PHONE AS PHONE, 
                                                         BCLIE_EMAIL AS EMAIL, 
                                                         BCLIE_CAREERS AS CAREERS
                                                    FROM BCLIE 
                                                   WHERE (UPPER(BCLIE_EMAIL) = UPPER(@EMAIL) 
                                                      OR REPLACE(REPLACE(REPLACE(BCLIE_PHONE, '(',''), ')',''), '-','') = @PHONE) 
                                                     AND UPPER(BCLIE_PASSWORD) = UPPER(@PASSWORD)
                                                     AND BCLIE_STATUS = 'A'");
                conn.Open();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@EMAIL", data.email);
                cmd.Parameters.AddWithValue("@PHONE", phone);
                cmd.Parameters.AddWithValue("@PASSWORD", data.password);
                MySqlDataReader rdr = cmd.ExecuteReader();
                rdr.Read();

                if (rdr.HasRows != true)
                {
                    serverMessage.Add(new serverError()
                    {
                        status = 300,
                        mensaje = "Las credenciales no son correctas."
                    });
                    return Ok(serverMessage);
                }
                else
                {
                    string token = appClass.getTOKEN();

                    dynamic myObject = new ExpandoObject();
                    myObject.id = rdr["BCLIE_ID"].ToString();
                    myObject.token = token;
                    myObject.name = rdr["NAME"].ToString();
                    myObject.lastName = rdr["LAST_NAME"].ToString();
                    myObject.email = rdr["EMAIL"].ToString();
                    myObject.phone = rdr["PHONE"].ToString();
                    myObject.Careers = rdr["CAREERS"].ToString();
                    myObject.password = "";

                    return Ok(myObject);
                }
            }
            catch (Exception ex)
            {
                serverMessage.Add(new serverError()
                {
                    status = 400,
                    mensaje = "Lo sentimos, ha ocurrido un error.",
                    mensajeServidor = ex.Message
                });
                return NotFound(serverMessage);
            }
        }

        //Crear nueva cuenta.
        [HttpPost]
        [Route("clie/registrarse")]
        public ActionResult postrRegistrarse([FromBody] client data)
        {
            var serverMessage = new List<serverError>();
            try
            {
                string validarEmail = appClass.retornarValor(@"SELECT 'X' FROM BCLIE WHERE UPPER(BCLIE_EMAIL) = '" + data.email.ToUpper() + "'");
                string validarPhone = appClass.retornarValor(@"SELECT 'X' FROM BCLIE WHERE UPPER(BCLIE_PHONE) = '" + data.phone + "'");

                if (validarEmail == "X")
                {
                    serverMessage.Add(new serverError()
                    {
                        status = 300,
                        mensaje = "No puede utilizar el email porque ya ha sido registrado. Si desea, puede recuperar su contraseña o iniciar sesión.",
                        mensajeServidor = "El email ya esta en uso."
                    });
                    return Ok(serverMessage);
                }
                else if (validarPhone == "X")
                {
                    serverMessage.Add(new serverError()
                    {
                        status = 300,
                        mensaje = "No puede utilizar el teléfono porque ya ha sido registrado. Si desea puede recuperar su contraseña o iniciar sesión.",
                        mensajeServidor = "El teléfono ya esta en uso."
                    });
                    return Ok(serverMessage);
                }
                else
                {
                    Guid CLIENT_ID = Guid.NewGuid();
                    conn.ConnectionString = appClass.GetConn();
                    MySqlCommand cmd = new MySqlCommand(@"INSERT INTO BCLIE (BCLIE_ID, BCLIE_NAME, BCLIE_LAST_NAME, BCLIE_EMAIL, BCLIE_PASSWORD, BCLIE_PHONE, BCLIE_CAREERS, BCLIE_STATUS, BCLIE_ACTIVITY_DATE)
                                                             VALUES (@BCLIE_ID, @BCLIE_NAME, @BCLIE_LAST_NAME, @BCLIE_EMAIL, @BCLIE_PASSWORD, @BCLIE_PHONE, @BCLIE_CAREERS, 'A', SYSDATETIME())");
                    cmd.Parameters.AddWithValue("@BCLIE_ID", CLIENT_ID);
                    cmd.Parameters.AddWithValue("@BCLIE_NAME", data.name);
                    cmd.Parameters.AddWithValue("@BCLIE_LAST_NAME", data.lastName);
                    cmd.Parameters.AddWithValue("@BCLIE_EMAIL", data.email);
                    cmd.Parameters.AddWithValue("@BCLIE_PASSWORD", data.password);
                    cmd.Parameters.AddWithValue("@BCLIE_PHONE", data.phone);
                    cmd.Parameters.AddWithValue("@BCLIE_CAREERS", data.Careers);
                    cmd.Connection = conn;
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    cmd.ExecuteNonQuery();

                    serverMessage.Add(new serverError()
                    {
                        status = 200,
                        mensaje = "Su registro ha sido completado.",
                        mensajeServidor = ""
                    });

                    return Ok(serverMessage);
                }
            }
            catch (Exception ex)
            {
                serverMessage.Add(new serverError()
                {
                    status = 400,
                    mensaje = "Lo sentimos, ha ocurrido un error.",
                    mensajeServidor = ex.Message
                });
                return Ok(serverMessage);
            }
        }

        [HttpPost]
        [Route("clie/actualizarInfo")]
        public ActionResult putActualizarInfo([FromBody] client data)
        {
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
                string password = appClass.retornarValor(@"SELECT BCLIE_PASSWORD FROM BCLIE WHERE BCLIE_ID = '" + data.id + "'");

                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"UPDATE BCLIE SET BCLIE_NAME = @NAME, 
                                                     BCLIE_LAST_NAME = @LAST_NAME, 
                                                     BCLIE_EMAIL = @EMAIL, 
                                                     BCLIE_PASSWORD = @PASSWORD, 
                                                     BCLIE_CAREERS = @CAREERS,
                                                     BCLIE_ACTIVITY_DATE =  SYSDATETIME()
                                               WHERE BCLIE_ID = @CLIENT_ID");
                cmd.Connection = conn;
                conn.Open();
                cmd.Parameters.AddWithValue("@NAME", data.name);
                cmd.Parameters.AddWithValue("@LAST_NAME", data.lastName);
                cmd.Parameters.AddWithValue("@EMAIL", data.email);
                if (data.password != "")
                    cmd.Parameters.AddWithValue("@PASSWORD", data.password);
                else
                    cmd.Parameters.AddWithValue("@PASSWORD", password);
                cmd.Parameters.AddWithValue("@CAREERS", data.Careers);
                cmd.Parameters.AddWithValue("@CLIENT_ID", data.id);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                cmd.ExecuteNonQuery();

                serverMessage.Add(new serverError()
                {
                    status = 200,
                    mensaje = "Su información ha sido actualizada.",
                    mensajeServidor = ""
                });

                return Ok(serverMessage);
            }
            catch (Exception ex)
            {
                serverMessage.Add(new serverError()
                {
                    status = 400,
                    mensaje = "Lo sentimos, ha ocurrido un error.",
                    mensajeServidor = ex.Message
                });
                return Ok(serverMessage);
            }
        }

        //Proxima cita
        [HttpPost]
        [Route("clie/citaActual")]
        public ActionResult postCitaActual([FromBody] citasCalendario data)
        {
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
                conn.ConnectionString = appClass.GetConn();

                MySqlCommand cmd = new MySqlCommand(@"SELECT BCALE_ID AS CALE_ID,
                                                     BCALE_START AS START_DATE,
                                                     BCALE_TRACKING AS TRACKING,
                                                     BCALE_STATUS AS STATUS,
                                                     BCALE_BEMPL_ID AS EMPL_ID
                                                FROM BCALE, BCLIE
                                               WHERE BCALE_CLIENT_ID = @CLIENT_ID
                                                 AND BCALE_CLIENT_ID = BCLIE_ID
                                                 AND BCALE_STATUS = 'A' 
                                                 AND BCALE_START > GETDATE()
                                               ORDER BY BCALE_START ASC");
                cmd.Connection = conn;
                conn.Open();
                cmd.Parameters.AddWithValue("@CLIENT_ID", data.idCliente);
                MySqlDataReader rdr = cmd.ExecuteReader();
                rdr.Read();
                dynamic citaActual = new ExpandoObject();
                if (rdr.HasRows == true)
                {
                    DateTime diaActual = DateTime.Now;
                    string fechaActual = diaActual.ToString("MM/dd/yyyy hh:mm tt");
                    string proximaCita = rdr["START_DATE"].ToString();

                    citaActual.proximaCita = Convert.ToDateTime(proximaCita);
                    citaActual.caleId = rdr["CALE_ID"].ToString();
                    citaActual.empId = rdr["EMPL_ID"].ToString();

                    if (Convert.ToDateTime(fechaActual) > Convert.ToDateTime(proximaCita))
                    {
                        citaActual.status = "300";
                        citaActual.mensaje = "No tiene una cita pendiente.";
                    }
                    else
                    {
                        citaActual.status = "200";
                        if (rdr["TRACKING"].ToString() == "C")
                        {
                            citaActual.mensaje = "(Cita confirmada)";
                            citaActual.statusCita = "C";
                        }
                        else
                        {
                            citaActual.mensaje = "(Su cita no ha sido confirmada)";
                            citaActual.statusCita = "I";
                        }
                    }
                }
                else
                {
                    citaActual.status = "201";
                    citaActual.statusCita = "";
                    citaActual.mensaje = "No tiene una cita pendiente.";
                }

                return Ok(citaActual);
            }
            catch (Exception ex)
            {
                serverMessage.Add(new serverError()
                {
                    status = 400,
                    mensaje = "Lo sentimos, ha ocurrido un error.",
                    mensajeServidor = ex.Message
                });
                return Ok(serverMessage);
            }
        }

        [HttpPost]
        [Route("clie/cancelarCita")]
        public ActionResult postCancelarCita([FromBody] citasCalendario data)
        {
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

                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"UPDATE BCALE SET BCALE_STATUS = 'I', 
                                                         BCALE_CLASSNAME = 'fc-danger', 
                                                         BCALE_ACTIVITY_DATE =  SYSDATETIME()
                                                   WHERE BCALE_USER_ID = @CLIENT_ID 
                                                     AND BCALE_ID = @BCALE_ID");
                cmd.Parameters.AddWithValue("@CLIENT_ID", data.idCliente);
                cmd.Parameters.AddWithValue("@BCALE_ID", data.idCalendario);
                cmd.Connection = conn;
                conn.Open();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                cmd.ExecuteNonQuery();

                serverMessage.Add(new serverError()
                {
                    status = 200,
                    mensaje = "Su cita ha sido cancelada.",
                    mensajeServidor = ""
                });
                return Ok(serverMessage);
            }
            catch (Exception ex)
            {
                serverMessage.Add(new serverError()
                {
                    status = 400,
                    mensaje = "Lo sentimos, ha ocurrido un error.",
                    mensajeServidor = ex.Message + " - " + ex.Data + " - " + ex.HResult + " - " + ex.ToString()
                });
                return Ok(serverMessage);
            }
        }

        //Todas las citas con paginacion
        [HttpGet]
        [Route("clie/misCitas")]
        //[ActionName("Usuario")]
        public IActionResult getUsuarioPage(string token, string idClient, int page, int PageSize)
        {
            var serverMessage = new List<serverError>();
            var citasCalendario = new List<citasCalendario>();

            try
            {
                if (appClass.validateToken(token) == false)
                {
                    serverMessage.Add(new serverError()
                    {
                        status = 100,
                        mensaje = "La sesión ha expirado.",
                        mensajeServidor = ""
                    });
                    return Ok(serverMessage);
                }

                int totalRows = Convert.ToInt32(appClass.retornarValor(@"SELECT COUNT(BCALE_ID) AS TotalRows FROM BCALE WHERE BCALE_CLIENT_ID = '" + idClient + "' AND BCALE_STATUS = 'A'"));

                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"SELECT BCALE_ID CALE_ID,
                                    BCLIE_ID AS CLIENT_ID, 
                                    BEMPL_ID AS EMP_ID, 
                                    BSHOP_ID AS BSHOP_ID, 
                                    CONCAT(BEMPL_NAME, ' ', BEMPL_LAST_NAME) AS EMPLEADO,
                                    BSHOP_NAME AS SHOP_NAME,
                                    BCALE_START AS START_DATE,
                                    CONVERT(DATETIME, FORMAT(BCALE_START, 'hh:mm tt')) AS HORA,
                                    BCALE_END AS START_END,
                                    BCALE_TRACKING AS TRACKING,
                                    BTRAT_ID AS RECIBO
                               FROM BCLIE, BEMPL, BSHOP, BCALE LEFT JOIN BTRAT ON BCALE_BSHOP_ID = BTRAT_BSHOP_ID AND BCALE_CLIENT_ID = BTRAT_BCLIE_ID AND BCALE_ID = BTRAT_BCALE_ID
                              WHERE BCALE_CLIENT_ID = @CLIENT_ID
                                AND BCALE_BEMPL_ID = BEMPL_ID
                                AND BCALE_CLIENT_ID = BCLIE_ID
                                AND BCALE_BSHOP_ID = BSHOP_ID
                                AND BCALE_STATUS = 'A'
                              ORDER BY BCALE_START DESC OFFSET @OffsetValue ROWS FETCH NEXT @PagingSize ROWS ONLY");
                cmd.Connection = conn;
                conn.Open();
                cmd.Parameters.AddWithValue("@CLIENT_ID", idClient);
                cmd.Parameters.AddWithValue("@OffsetValue", (page - 1) * PageSize);
                cmd.Parameters.AddWithValue("@PagingSize", PageSize);
                MySqlDataReader rdr = cmd.ExecuteReader();

                var listaServicios = new List<serviciosCitas>();
                while (rdr.Read())
                {
                    //Buscar los servicios de la cita
                    MySqlCommand cmdServ = new MySqlCommand(@"SELECT BCALE_ID AS ID,
                                                                          BSERV_NAME AS SERVICIO, 
									                                      BSERV_PRICE AS PRECIO, 
									                                      BSERV_TIME AS TIEMPO,
                                                                        BSERV_DESCRIPTION AS DESCRIPTION
						                                             FROM BCALE, BSERV, RSCAL, BCLIE
								                                    WHERE BCALE_ID = @BCALE_ID
								                                      AND BCALE_ID = RSCAL_BCALE_ID
								                                      AND BSERV_ID = RSCAL_RSERV_ID
								                                      AND BCLIE_ID LIKE @BCLIE_ID");
                    cmdServ.Connection = conn;
                    cmdServ.Parameters.AddWithValue("@BCALE_ID", rdr["CALE_ID"].ToString());
                    cmdServ.Parameters.AddWithValue("@BCLIE_ID", idClient);
                    MySqlDataReader rdrServ = cmdServ.ExecuteReader();

                    listaServicios.Clear();
                    while (rdrServ.Read())
                    {
                        listaServicios.Add(new serviciosCitas()
                        {
                            id = rdrServ["ID"].ToString(),
                            services = rdrServ["SERVICIO"].ToString(),
                            price = rdrServ["PRECIO"].ToString(),
                            time = rdrServ["TIEMPO"].ToString(),
                            description = rdrServ["DESCRIPTION"].ToString()
                        });
                    }

                    citasCalendario.Add(new citasCalendario()
                    {
                        idCompania = rdr["BSHOP_ID"].ToString(),
                        idCalendario = rdr["CALE_ID"].ToString(),
                        idEmpleado = rdr["EMP_ID"].ToString(),
                        idCliente = rdr["CLIENT_ID"].ToString(),
                        nombreEmpleado = rdr["EMPLEADO"].ToString(),
                        nombreLocal = rdr["SHOP_NAME"].ToString(),
                        fechaComienza = rdr["START_DATE"].ToString(),
                        hora = rdr["HORA"].ToString(),
                        fechaTermina = rdr["START_END"].ToString(),
                        tracking = rdr["TRACKING"].ToString(),
                        recibo = rdr["RECIBO"].ToString(),
                        servicios = listaServicios
                        //clsUsuario.price = Convert.IsDBNull(ds.Tables[0].Rows[i]["NivelSeg"]) ? "" : Convert.ToString(ds.Tables[0].Rows[i]["NivelSeg"]);
                    });
                }

                dynamic result = new ExpandoObject();
                var pager = new paginacion(totalRows, page, 10, 10);

                result.citasCalendario = citasCalendario;
                result.paginacion = pager;

                return Ok(result);
            }
            catch (Exception ex)
            {
                serverMessage.Add(new serverError()
                {
                    status = 400,
                    mensaje = "Lo sentimos, ha ocurrido un error.",
                    mensajeServidor = ex.Message + " - " + ex.Data + " - " + ex.HResult + " - " + ex.ToString()
                });
                return Ok(serverMessage);
            }
            finally
            {
                conn.Close();
            }
        }

    }

}