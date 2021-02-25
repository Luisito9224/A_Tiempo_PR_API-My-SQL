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

namespace A_Tiempo_PR_API.Controllers.clientes
{
    [ApiController]
    public class localesController : Controller
    {
        MySqlConnection conn = new MySqlConnection();
        private MainClass appClass = new MainClass();

        //Buscar todos los locales
        [HttpGet]
        [Route("clie/allLocal")]
        public ActionResult GetAllLocal(string id)
        {
            try
            {
                if (id == null)
                    id = "%";
                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"SELECT BSHOP_ID AS ID,
                                                     BSHOP_NAME AS NAME, 
                                                     CASE 
                                                        WHEN BSHOP_BANNER IS NULL OR BSHOP_BANNER = '' THEN 'logo/' + BSHOP_LOGO
                                                        ELSE BSHOP_BANNER 
                                                     END AS BANNER, 
                                                     BSHOP_DESCRIPTION AS DESCRIPTION,
                                                     BSHOP_GMAPS AS GMAPS, 
                                                     BSHOP_FACEBOOK AS FACEBOOK, 
                                                     BSHOP_INSTAGRAM AS INSTAGRAM,
                                                     BSHOP_EMAIL AS EMAIL, 
                                                     BSHOP_PHONE AS PHONE
                                                FROM BSHOP, RSUBC
                                               WHERE BSHOP_STATUS = 'A'
                                                 AND RSUBC_STATUS = 'A' 
                                                 AND RSUBC_BSHOP_ID = BSHOP_ID
                                                 AND BSHOP_ID LIKE @BSHOP_ID
                                                 AND RSUBC_BSUBC_ID <> 'FREE_M'
                                                 AND CONVERT(DATETIME, FORMAT(RSUBC_PLAN_END, 'MM/dd/yyyy')) >= CONVERT(DATETIME, FORMAT(SYSDATETIME(), 'MM/dd/yyyy'))
                                                 AND RSUBC_ID = (SELECT MAX(RSUBC_ID) FROM RSUBC B WHERE B.RSUBC_BSHOP_ID = BSHOP_ID)");
                conn.Open();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@BSHOP_ID", id);
                MySqlDataReader rdr = cmd.ExecuteReader();

                var lista = new List<locales>();
                while (rdr.Read())
                {
                    lista.Add(new locales()
                    {
                        id = rdr["ID"].ToString(),
                        name = rdr["NAME"].ToString(),
                        banner = rdr["BANNER"].ToString(),
                        description = rdr["DESCRIPTION"].ToString(),
                        googleMaps = rdr["GMAPS"].ToString(),
                        faceBook = rdr["FACEBOOK"].ToString(),
                        instagram = rdr["INSTAGRAM"].ToString(),
                        email = rdr["EMAIL"].ToString(),
                        phone = rdr["PHONE"].ToString(),
                    });
                }

                return Ok(lista);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        //Buscar todos los empleados del local
        [HttpGet]
        [Route("clie/allEmpFromLocal")]
        public ActionResult GetAllProfesionalFromLocal(string id)
        {
            try
            {
                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"SELECT DISTINCT BEMPL_ID AS ID,
                                                     CONCAT(BEMPL_NAME, ' ', BEMPL_LAST_NAME) AS FULLNAME,
                                                     BEMPL_NAME AS NAME,
                                                     BEMPL_LAST_NAME AS LAST_NAME,
                                                     BEMPL_PHONE AS PHONE,
                                                     BEMPL_CAREERS AS CAREERS,
                                                     BEMPL_EMAIL AS EMAIL,
                                                     BEMPL_PASSWORD AS PASSWORD,
                                                     BEMPL_INSTAGRAM AS INSTAGRAM,
                                                     BEMPL_FACEBOOK AS FACEBOOK,
                                                     CASE WHEN BEMPL_AVATAR = '' OR BEMPL_AVATAR IS NULL THEN 'avatar.png' ELSE BEMPL_AVATAR END AS AVATAR,
                                                     BEMPL_COMMENT AS COMMENT,
                                                     BEMPL_ROLE AS ROLE,
                                                     BEMPL_STATUS AS STATUS,
                                                     BEMPL_ACTIVITY_DATE AS ACTIVITY_DATE
                                                FROM BEMPL, RSERV
                                               WHERE BEMPL_BSHOP_ID = @SHOP_ID
                                                 AND BEMPL_ID LIKE '%'
                                                 AND BEMPL_STATUS = 'A'
                                                 AND RSERV_BEMPL_ID = BEMPL_ID
                                               ORDER BY BEMPL_NAME ASC");
                conn.Open();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@SHOP_ID", id);
                MySqlDataReader rdr = cmd.ExecuteReader();

                var lista = new List<profesionales>();
                while (rdr.Read())
                {
                    lista.Add(new profesionales()
                    {
                        Id = rdr["ID"].ToString(),
                        Name = rdr["NAME"].ToString(),
                        LastName = rdr["LAST_NAME"].ToString(),
                        Phone = rdr["PHONE"].ToString(),
                        Email = rdr["EMAIL"].ToString(),
                        Carreers = rdr["CAREERS"].ToString(),
                        Instagram = rdr["INSTAGRAM"].ToString(),
                        Facebook = rdr["FACEBOOK"].ToString(),
                        Avatar = rdr["AVATAR"].ToString(),
                        Comment = rdr["COMMENT"].ToString(),
                        Status = rdr["STATUS"].ToString(),
                        Role = rdr["ROLE"].ToString()
                    });
                }
                return Ok(lista);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        //Mostrar el horario del local
        [HttpGet]
        [Route("clie/hourFromLocal")]
        public ActionResult GetHoursLocal(string id)
        {
            try
            {
                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"SELECT BSHOU_ID AS ID,
                                                     BSHOU_DATE AS DIA, 
                                                     FORMAT(CONVERT(datetime, BSHOU_OPEN), 'hh:mm tt') AS H_OPEN, 
                                                     FORMAT(CONVERT(datetime, BSHOU_CLOSE), 'hh:mm tt') AS H_CLOSE,
                                                     CASE WHEN BSHOU_STATUS = 'I' THEN 'Cerrado' ELSE 'Abierto' END AS STATUS,
                                                     BSHOU_ACTIVITY_DATE AS ACTIVITY_DATE
                                                FROM BSHOU
                                               WHERE BSHOU_BSHOP_ID = @SHOP_ID AND BSHOU_STATUS = 'A' ORDER BY BSHOU_ID ASC");
                conn.Open();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@SHOP_ID", id);
                MySqlDataReader rdr = cmd.ExecuteReader();

                var lista = new List<horarios>();
                while (rdr.Read())
                {
                    lista.Add(new horarios()
                    {
                        Id = rdr["ID"].ToString(),
                        Open = rdr["H_OPEN"].ToString(),
                        Close = rdr["H_CLOSE"].ToString(),
                        Date = rdr["DIA"].ToString()
                    });
                }
                return Ok(lista);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        //Mostrar si al momento esta abierto o cerrado
        [HttpGet]
        [Route("clie/openOrClose")]
        public ActionResult GetOpenOrCloseDate(string id)
        {
            try
            {
                CultureInfo ci = new CultureInfo("es-PR");
                DateTime diaActual = DateTime.Now;
                string dia = diaActual.ToString("dddd", ci);
                string tiempoActual = diaActual.ToString("HH:mm", ci);

                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"SELECT FORMAT(CONVERT(datetime, BSHOU_CLOSE), 'hh:mm tt') AS OPEN_OR_CLOSE
                                                FROM BSHOU WHERE UPPER(BSHOU_DATE) = UPPER(@DIA) 
                                                 AND CONVERT(datetime, @ACTUAL_TIME) BETWEEN FORMAT(CONVERT(datetime, BSHOU_OPEN), 'HH:mm') 
                                                 AND FORMAT(CONVERT(datetime, BSHOU_CLOSE), 'HH:mm') 
                                                 AND BSHOU_BSHOP_ID = @SHOP_ID 
                                                 AND BSHOU_STATUS = 'A'");
                conn.Open();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@DIA", dia);
                cmd.Parameters.AddWithValue("@ACTUAL_TIME", tiempoActual);
                cmd.Parameters.AddWithValue("@SHOP_ID", id);
                MySqlDataReader rdr = cmd.ExecuteReader();

                var lista = new List<horarios>();
                while (rdr.Read())
                {
                    lista.Add(new horarios()
                    {
                        DateActualOpenOrClose = rdr["OPEN_OR_CLOSE"].ToString()
                    });
                }
                return Ok(lista);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        //Mostrar los servicios del local
        [HttpGet]
        [Route("clie/localServices")]
        public ActionResult GetServices(string id)
        {
            try
            {
                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"SELECT BSERV_ID AS ID,
                                                     BSERV_NAME AS SERVICIO, 
                                                     BSERV_TIME AS TIEMPO, 
                                                     BSERV_PRICE AS PRECIO, 
                                                     BSERV_DESCRIPTION AS DESCRIPTION,
                                                     BSERV_ACTIVITY_DATE AS ACTIVITY_DATE
                                                FROM BSERV
                                               WHERE BSERV_BSHOP_ID = @SHOP_ID AND BSERV_STATUS = 'A' ORDER BY BSERV_SORT, BSERV_NAME");
                conn.Open();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@SHOP_ID", id);
                MySqlDataReader rdr = cmd.ExecuteReader();

                var lista = new List<servicios>();
                while (rdr.Read())
                {
                    lista.Add(new servicios()
                    {
                        id = rdr["ID"].ToString(),
                        services = rdr["SERVICIO"].ToString(),
                        price = rdr["PRECIO"].ToString(),
                        time = rdr["TIEMPO"].ToString(),
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

        //Mostrar galería
        [HttpGet]
        [Route("clie/gallery")]
        public ActionResult GetGallery(string id)
        {
            try
            {
                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"SELECT BGALE_IMAGE AS IMAGEN, 
                                                         BGALE_TITLE AS TITLE, 
                                                         BGALE_DESCRIPTION AS DESCRIPTION 
                                                    FROM BGALE 
                                                   WHERE BGALE_BSHOP_ID = @SHOP_ID");
                conn.Open();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@SHOP_ID", id);
                MySqlDataReader rdr = cmd.ExecuteReader();

                var lista = new List<gallery>();
                while (rdr.Read())
                {
                    lista.Add(new gallery()
                    {
                        imageUrl = rdr["IMAGEN"].ToString(),
                        title = rdr["TITLE"].ToString(),
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

        //Mostrar procedimientos de COVID
        [HttpGet]
        [Route("clie/covid")]
        public ActionResult GetCovid(string id)
        {
            try
            {
                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"SELECT COVID_CONTROL AS CONTROL, 
                                                         COVID_ICON AS ICON,
                                                         (SELECT CONCAT(BEMPL_NAME, ' ', BEMPL_LAST_NAME) AS NAME FROM BEMPL WHERE BEMPL_ID = COVID_USER_ID) AS USER_ID, 
                                                         COVID_ACTIVITY_DATE AS ACTIVITY_DATE
                                                    FROM COVID
                                                   WHERE COVID_BSHOP_ID = @SHOP_ID 
                                                ORDER BY COVID_SORT, COVID_CONTROL");
                conn.Open();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@SHOP_ID", id);
                MySqlDataReader rdr = cmd.ExecuteReader();

                var lista = new List<covid>();
                while (rdr.Read())
                {
                    lista.Add(new covid()
                    {
                        process = rdr["CONTROL"].ToString(),
                        icon = rdr["ICON"].ToString()
                    });
                }
                return Ok(lista);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        //Buscar los locales del cliente
        [HttpGet]
        [Route("clie/localClient")]
        public ActionResult GetLocalClient(string id, string token)
        {
            var serverMessage = new List<serverError>();
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
                conn.ConnectionString = appClass.GetConn();
                MySqlCommand cmd = new MySqlCommand(@"SELECT DISTINCT BSHOP_SHARE_LINK AS SHARE_LINK, 
                                                         BSHOP_NAME AS NAME,
                                                         BSHOP_ID AS ID, 
                                                         CASE 
                                                            WHEN BSHOP_BANNER IS NULL OR BSHOP_BANNER = '' THEN 'logoApp.png' 
                                                            ELSE 'banner/' + BSHOP_BANNER 
                                                         END AS BANNER, 
                                                         BSHOP_DESCRIPTION AS DESCRIPTION 
                                                    FROM BSHOP, BCLIE, BCALE, RSUBC
                                                   WHERE BSHOP_STATUS = 'A' 
											  	     AND BCALE_CLIENT_ID = BCLIE_ID
												     AND BCALE_BSHOP_ID = BSHOP_ID
												     AND BCLIE_ID = @CLIENT_ID
                                                     AND RSUBC_STATUS = 'A' 
                                                     AND RSUBC_BSHOP_ID = BSHOP_ID
                                                     AND RSUBC_BSUBC_ID <> 'FREE_M'
                                                     AND CONVERT(DATETIME, FORMAT(RSUBC_PLAN_END, 'MM/dd/yyyy')) >= CONVERT(DATETIME, FORMAT(SYSDATETIME(), 'MM/dd/yyyy'))
                                                     AND RSUBC_ID = (SELECT MAX(RSUBC_ID) FROM RSUBC B WHERE B.RSUBC_BSHOP_ID = BSHOP_ID)");
                conn.Open();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@CLIENT_ID", id);
                MySqlDataReader rdr = cmd.ExecuteReader();

                var lista = new List<locales>();
                while (rdr.Read())
                {
                    lista.Add(new locales()
                    {
                        id = rdr["ID"].ToString(),
                        name = rdr["NAME"].ToString(),
                        banner = rdr["BANNER"].ToString(),
                        description = rdr["DESCRIPTION"].ToString()
                    });
                }

                return Ok(lista);
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
    }
}
