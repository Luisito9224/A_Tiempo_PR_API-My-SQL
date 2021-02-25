using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Net.Mail;
using System.Text;
using System.IO;
using System.Globalization;

namespace atiempo_api.App_Class
{
    public class MainClass
    {
        public MySqlConnection conn = new MySqlConnection();
        public string GetConn()
        {
            string server = "atiempodatabase-do-user-8773380-0.b.db.ondigitalocean.com"; //Nombre o ip del servidor de MySQL
            string bd = "atiempopr"; //Nombre de la base de datos
            string usuario = "luillo9224"; //Usuario de acceso a MySQL
            string password = "pkzjqmdyhulm3p6c"; //Contraseña de usuario de acceso a MySQL
            return @"Server=" + server + "; Port=25060; Database=" + bd + "; Uid=" + usuario + "; Pwd=" + password + ";Port = 25060;";
        }

        public string getTOKEN()
        {
            return "ATPR-6C2B0FB0-79F5-49DF-8490-0D519065251B.dd5f18b3-2eef-47c8-80b3-c05e0ffced1d";
        }
        public bool validateToken(string token)
        {
            try
            {
                if (token != getTOKEN())
                    return false;
                else return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public string retornarValor(string query)
        {
            string valor = "Error:";
            try
            {
                MySqlCommand cmd = new MySqlCommand(query);
                cmd.Connection = conn;
                if (conn.State == ConnectionState.Closed)
                {
                    conn.ConnectionString = GetConn();
                    conn.Open();
                }
                MySqlDataReader rdr = cmd.ExecuteReader();
                rdr.Read();
                if (rdr.HasRows == true)
                {
                    valor = rdr[0].ToString();
                }
                else
                    valor = "";
                rdr.Close();
            }
            catch (Exception ex)
            {
                string error = ex.Message.Replace("\n", ". ").Replace("'", "").Replace(".\r", "");
                valor = "Error: " + error;
            }
            finally
            {
                conn.Close();
            }
            return valor;
        }
        public bool existe(string query)
        {
            bool enocntrado = false;
            try
            {
                MySqlCommand cmd = new MySqlCommand(query);
                if (conn.State == ConnectionState.Closed)
                {
                    conn.ConnectionString = GetConn();
                    conn.Open();
                }
                cmd.Connection = conn;
                MySqlDataReader rdr = cmd.ExecuteReader();
                rdr.Read();
                if (rdr.HasRows == true)
                {
                    enocntrado = true;
                }
                else
                    enocntrado = false;
                rdr.Close();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                enocntrado = false;
            }
            finally
            {
                conn.Close();
            }
            return enocntrado;
        }


        public string sendEmail(string emailTo, string mensaje, string subject)
        {
            //FUNCION PARA ENVIAR EMAILS CON ARCHIVO
            try
            {
                MailMessage mail = new MailMessage();
                StringBuilder EMailBody = new System.Text.StringBuilder();
                mail.To.Add(emailTo);
                mail.From = new MailAddress("alerta@atiempopr.com");
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.BodyEncoding = System.Text.Encoding.UTF8;

                string path = "";//HttpContext.Current.Server.MapPath("assets/emailTemplate/index.html");
                StreamReader reader = new StreamReader(path);
                string template = reader.ReadToEnd();
                reader.Close();

                template = template.Replace("[#body]", mensaje + "<br/><br/><br/>Favor de no contestar este email.");
                //template = template.Replace("[#banner]", "assets/emailTemplate/" + banner);
                EMailBody.Append(template);
                AlternateView HTMLEmail = AlternateView.CreateAlternateViewFromString(EMailBody.ToString(), null, "text/html");
                mail.AlternateViews.Add(HTMLEmail);
                SmtpClient client = new SmtpClient("smtp.ionos.com");
                client.UseDefaultCredentials = true;
                client.Credentials = new System.Net.NetworkCredential("info@luisbauzo.com", "Luillo9224!");
                client.Send(mail);
                mail.Dispose();
                return "";
            }
            catch (Exception ex)
            {
                string error = ex.Message.Replace("\n", ". ").Replace("'", "").Replace(".\r", "");
                return "Error: " + error;
            }
        }

        public string sendText(string id, string subject, string mensaje)
        {
            try
            {
                //Buscamos el número de teléfono o el email
                string phone = retornarValor(@"SELECT CASE WHEN BCLIE_CAREERS = '' OR BCLIE_CAREERS IS NULL THEN '' 
                                                           ELSE BCLIE_PHONE + (SELECT MCAREERS_VALUE FROM MCAREERS WHERE MCAREERS_ID = BCLIE_CAREERS) 
                                                       END AS PHONE
                                                  FROM BCLIE
                                                  WHERE BCLIE_ID = '" + id + "'");
                if (phone == "")
                {
                    string email = retornarValor(@"SELECT BCLIE_EMAIL AS EMAIL
                                                     FROM BCLIE
                                                    WHERE BCLIE_ID = '" + id + "'");

                    MailMessage mail = new MailMessage();
                    StringBuilder EMailBody = new System.Text.StringBuilder();
                    mail.To.Add(email);
                    mail.From = new MailAddress("alerta@atiempopr.com");
                    mail.Subject = subject;
                    mail.IsBodyHtml = true;
                    mail.BodyEncoding = System.Text.Encoding.UTF8;

                    string path = "";//HttpContext.Current.Server.MapPath("assets/emailTemplate/index.html");
                    StreamReader reader = new StreamReader(path);
                    string template = reader.ReadToEnd();
                    reader.Close();

                    template = template.Replace("[#body]", mensaje + "<br/><br/><br/>Favor de no contestar este email.");
                    template = template.Replace("[#banner]", "assets/emailTemplate/banner.png");
                    EMailBody.Append(template);
                    AlternateView HTMLEmail = AlternateView.CreateAlternateViewFromString(EMailBody.ToString(), null, "text/html");
                    mail.AlternateViews.Add(HTMLEmail);
                    SmtpClient client = new SmtpClient("smtp.ionos.com");
                    client.UseDefaultCredentials = true;
                    client.Credentials = new System.Net.NetworkCredential("info@luisbauzo.com", "Luillo9224!");
                    client.Send(mail);
                    mail.Dispose();
                    return "";
                }
                else
                {
                    //Enviamos el mensaje de texto.
                    MailMessage email = new MailMessage();
                    phone = phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Trim();
                    email.To.Add(new MailAddress(phone));
                    email.From = new MailAddress("alerta@atiempopr.com");
                    subject = subject.ToUpper().Replace("Á", "").Replace("É", "").Replace("Í", "").Replace("Ó", "").Replace("Ú", "");
                    email.Subject = subject;
                    mensaje = mensaje.ToUpper().Replace("Á", "").Replace("É", "").Replace("Í", "").Replace("Ó", "").Replace("Ú", "");
                    email.Body = mensaje;
                    email.IsBodyHtml = false;
                    email.Priority = MailPriority.Normal;

                    SmtpClient smtp = new SmtpClient("smtp.ionos.com");
                    smtp.EnableSsl = false;
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = new System.Net.NetworkCredential("info@luisbauzo.com", "Luillo9224!");

                    smtp.Send(email);
                    email.Dispose();

                    return "";
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return error;
            }
        }

        //Eliminar las horas que estan comprometidas
        public string ocultarhorasComprometidas(string date, string hora, string SHOP_ID, string EMP_ID, string timeEstimado, DateTime diaHoraActualMilitar, DateTime diahoraSelectMilitar)
        {
            try
            {
                //Eliminar las horas antes si escogio el dia actual.
                CultureInfo ci = new CultureInfo("es-PR");
                TimeSpan TIEMPO_ESTIMADO = Convert.ToDateTime(timeEstimado, ci).TimeOfDay;
                TimeZoneInfo puertoRico = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                string fecha = date.Replace(".", "/");
                DateTime diaActual = TimeZoneInfo.ConvertTime(DateTime.Now, puertoRico);
                //string diaSemanaActual = diaActual.ToString("dddd");
                string diaSemanaSelect = diahoraSelectMilitar.ToString("dddd");

                //No puede seleccionar un dia anterior al actual
                if (diaHoraActualMilitar > diahoraSelectMilitar)
                    return "Anterior a hoy: " + hora;

                //Eliminar horas antes de la apertura y despues del cierre
                if (existe(@"SELECT 'X' FROM BSHOU WHERE BSHOU_BSHOP_ID = '" + SHOP_ID + @"' 
                                      AND CONVERT(datetime, '" + hora + @"') BETWEEN FORMAT(CONVERT(datetime, BSHOU_OPEN), 'hh:mm tt') AND FORMAT(CONVERT(datetime, BSHOU_CLOSE), 'hh:mm tt')
                                      AND UPPER(BSHOU_DATE) = '" + diaSemanaSelect.ToUpper() + @"'
                                      AND BSHOU_STATUS = 'A'") == false)
                {
                    return "Horas antes de la apertura y cierre: " + hora + " semana " + diaSemanaSelect;
                }

                //Eliminar hora antes de la entrada del empleado y despues de la salida del empleado
                if (existe(@"SELECT 'X' FROM BEHOU WHERE BEHOU_BEMPL_ID = '" + EMP_ID + @"' 
                                     AND CONVERT(datetime, '" + hora + @"') BETWEEN FORMAT(CONVERT(datetime, BEHOU_OPEN), 'hh:mm tt') AND FORMAT(CONVERT(datetime, BEHOU_CLOSE), 'hh:mm tt')
                                     AND UPPER(BEHOU_DATE) = '" + diaSemanaSelect.ToUpper() + @"'
                                     AND BEHOU_STATUS = 'A'") == false)
                {
                    return "Antes de entrar el empleado: " + hora;
                }

                //Eliminar horas de break
                DateTime h = Convert.ToDateTime(hora, ci).AddMinutes(1);
                if (existe(@"SELECT 'X' FROM BEHOU WHERE BEHOU_BEMPL_ID = '" + EMP_ID + @"' 
                                     AND CONVERT(datetime, '" + h.ToString("HH:mm", ci) + @"') BETWEEN FORMAT(CONVERT(datetime, BEHOU_BREAK_START), 'HH:mm') AND FORMAT(CONVERT(datetime, BEHOU_BREAK_END), 'hh:mm tt')
                                     AND UPPER(BEHOU_DATE) = UPPER('" + diaSemanaSelect + @"')
                                     AND BEHOU_STATUS = 'A'") == true)
                {
                    return "Break: " + hora;
                }

                string cierre = retornarValor(@"SELECT BEHOU_CLOSE FROM BEHOU WHERE BEHOU_BEMPL_ID = '" + EMP_ID + @"' 
                                                        AND UPPER(BEHOU_DATE) = '" + diaSemanaSelect.ToUpper() + "' AND BEHOU_STATUS = 'A'");
                string breakH = retornarValor(@"SELECT BEHOU_BREAK_START FROM BEHOU WHERE BEHOU_BEMPL_ID = '" + EMP_ID + @"' 
                                                        AND UPPER(BEHOU_DATE) = '" + diaSemanaSelect.ToUpper() + "' AND BEHOU_STATUS = 'A'");
                DateTime horaDeCierre = Convert.ToDateTime(cierre, ci);
                DateTime horaDeBreak = Convert.ToDateTime(breakH, ci);

                //Validar que el tiempo estimado no conflija con la hora de break
                TimeSpan diferencia = horaDeBreak.Subtract(Convert.ToDateTime(hora));
                if (diferencia < TIEMPO_ESTIMADO && diferencia.ToString().Contains("-") == false)
                    return "tiempo estimado no conflija con break: " + hora;

                //Validar que el tiempo estimado no conflija con la hora de cierre
                diferencia = horaDeCierre.Subtract(Convert.ToDateTime(hora, ci));
                if (diferencia < TIEMPO_ESTIMADO && diferencia.ToString().Contains("-") == false)
                    return "tiempo estimado no conflija con cierre: " + hora;

                //Eliminar horas comprometidas
                h = Convert.ToDateTime(hora, ci).AddMinutes(1);
                if (existe(@"SELECT 'X' FROM BCALE 
                                   WHERE BCALE_BEMPL_ID = '" + EMP_ID + @"' 
                                     AND CONVERT(datetime, '" + fecha.ToUpper() + " " + h.ToString("HH:mm", ci) + @"') BETWEEN FORMAT(CONVERT(datetime, BCALE_START), 'MM/dd/yyyy HH:mm') AND FORMAT(CONVERT(datetime, BCALE_END), 'MM/dd/yyyy HH:mm')
                                     AND BCALE_STATUS = 'A'") == true)
                {
                    return "eliminar horas comprometidas: " + hora;
                }

                //Validar que el tiempo del servicio no conflija con el tiempo de otra cita. El espacio disponible debe ser mayor al tiempo del servicio.
                MySqlCommand cmd = new MySqlCommand(@"SELECT FORMAT(CONVERT(datetime, BCALE_START), 'hh:mm tt') AS START_DAY, 
                                                     FORMAT(CONVERT(datetime, BCALE_END), 'hh:mm tt') AS END_DAY, 
                                                     FORMAT(CONVERT(datetime, UPPER(BCALE_START)), 'dddd') AS DIA 
                                                FROM BCALE 
                                               WHERE BCALE_BEMPL_ID = '" + EMP_ID + @"'
                                                 AND FORMAT(CONVERT(datetime, BCALE_START), 'MM/dd/yyyy') = FORMAT(CONVERT(datetime, '" + fecha.ToUpper() + @"'), 'MM/dd/yyyy')
                                                 AND BCALE_STATUS = 'A'");

                if (conn.State == ConnectionState.Closed)
                {
                    conn.ConnectionString = GetConn();
                    conn.Open();
                }
                cmd.Connection = conn;
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    DateTime START = Convert.ToDateTime(rdr[0].ToString(), ci);
                    DateTime END = Convert.ToDateTime(rdr[1].ToString(), ci);
                    if (Convert.ToDateTime(hora, ci) < START)
                    {
                        diferencia = START.Subtract(Convert.ToDateTime(hora, ci));
                        if (diferencia < TIEMPO_ESTIMADO)
                            return "No conflija con otra cita: " + hora;
                    }
                }

                //Validar que el tiempo del servicio no conflija con el tiempo parcial. El espacio disponible debe ser mayor al tiempo del servicio.
                cmd = new MySqlCommand(@"SELECT FORMAT(CONVERT(datetime, BHOLI_START), 'hh:mm tt') AS START_DAY, 
                                                     FORMAT(CONVERT(datetime, BHOLI_END), 'hh:mm tt') AS END_DAY, 
                                                     FORMAT(CONVERT(datetime, UPPER(BHOLI_START)), 'dddd') AS DIA 
                                                FROM BHOLI 
                                               WHERE BHOLI_BEMPL_ID = '" + EMP_ID + @"'
                                                 AND FORMAT(CONVERT(datetime, BHOLI_START), 'MM/dd/yyyy') = FORMAT(CONVERT(datetime, '" + fecha.ToUpper() + @"'), 'MM/dd/yyyy')
                                                 AND BHOLI_STATUS = 'A'");
                if (conn.State == ConnectionState.Closed)
                {
                    conn.ConnectionString = GetConn();
                    conn.Open();
                }
                cmd.Connection = conn;
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    DateTime START = Convert.ToDateTime(rdr[0].ToString(), ci);
                    DateTime END = Convert.ToDateTime(rdr[1].ToString(), ci);
                    if (Convert.ToDateTime(hora, ci) < START)
                    {
                        diferencia = START.Subtract(Convert.ToDateTime(hora, ci));
                        if (diferencia < TIEMPO_ESTIMADO)
                            return "No conflija con tiempo parcial: " + hora;
                    }
                }

                //Eliminar las horas de Tiempo Parcial
                h = Convert.ToDateTime(hora, ci).AddMinutes(1);
                if (existe(@"SELECT 'X' FROM BHOLI 
                                   WHERE BHOLI_BEMPL_ID = '" + EMP_ID + @"' 
                                     AND CONVERT(datetime, '" + fecha.ToUpper() + " " + h.ToString("HH:mm", ci) + @"') BETWEEN FORMAT(CONVERT(datetime, BHOLI_START), 'MM/dd/yyyy HH:mm') AND FORMAT(CONVERT(datetime, BHOLI_END), 'MM/dd/yyyy HH:mm')
                                     AND BHOLI_STATUS = 'A'") == true)
                {
                    return "Horas de tiempo parcial: " + hora;
                }

            }
            catch (Exception ex)
            {
                string error = ex.Message + " - " + ex.Source + " - " + ex.InnerException + " - " + ex.ToString();
                sendEmail("luisbauzo@outlook.com", "Ha ocurrido el siguiente error: " + error, "Error en el API");
                return error;
            }
            return "Y";
        }

        public string ejecutarScript(string query)
        {
            string valor = "Error:";
            bool cerrar = false;
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.ConnectionString = GetConn();
                    conn.Open();
                    cerrar = true;
                }
                MySqlCommand cmd = new MySqlCommand(query);
                cmd.Connection = conn;
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                cmd.ExecuteNonQuery();
                valor = "";
            }
            catch (Exception ex)
            {
                string error = ex.Message.Replace("\n", ". ").Replace("'", "").Replace(".\r", "");
                valor = "Error: " + error;
            }
            finally
            {
                if (cerrar == true)
                    conn.Close();
            }
            return valor;
        }

        public void enviarNotificaciones()
        {
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
                    conn.ConnectionString = GetConn();
                    conn.Open();
                }
                cmd.Connection = conn;
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string emailCliente = rdr["CLIENT_EMAIL"].ToString();
                    string mensajeTextoCliente = rdr["TEST_MESSAGE"].ToString();
                    string nombreCliente = rdr["CLIENT_NAME"].ToString();
                    string nombreEmpleado = rdr["EMPLOYE_NAME"].ToString();
                    string nombreLocal = rdr["SHOP_NAME"].ToString();
                    string clientID = rdr["CLIENT_ID"].ToString();
                    string CitaID = rdr["CALE_ID"].ToString();

                    string validar = ejecutarScript(@"UPDATE BCALE SET BCALE_NOTIFY = 1, BCALE_ACTIVITY_DATE =  SYSDATETIME() WHERE BCALE_CLIENT_ID = '" + clientID + "' AND BCALE_ID = '" + CitaID + "'");
                    if (validar == "")
                    {
                        //Enviar email al cliente.
                        string mensaje = @"Saludos,<br/><br/>Deseamos recordarle que tiene una cita el " + rdr["FECHA_START"].ToString() + " a las " + rdr["START_H"].ToString() + " con " + nombreEmpleado + ". Le estaremos esperando.";
                        string send = sendEmail(emailCliente, mensaje, "Recordatorio de cita");

                        //Enviar mensaje de texto al empleado
                        if (mensajeTextoCliente != "")
                            sendText(mensajeTextoCliente, "Saludos, deseamos recordarle que tiene una cita el " + rdr["FECHA_START"].ToString() + " a las " + rdr["START_H"].ToString() + " con " + nombreEmpleado + ". Le estaremos esperando.", "Recordatorio de cita");
                    }
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                string error = ex.Message.Replace("\n", ". ").Replace("'", "").Replace(".\r", "");
            }
            finally
            {
                conn.Close();
            }
        }
    }
}