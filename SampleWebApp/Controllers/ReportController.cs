using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;

namespace SampleWebApp.Controllers
{
    public class ReportController : Controller
    {
        private DataRow CreateAuthToken()
        {
            string userId = User.Identity.GetUserId();

            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            SqlCommand command = new SqlCommand("[dbo].[CreateSSRSAuthToken]", connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            SqlDataAdapter adapter = new SqlDataAdapter(command);

            DataSet results = new DataSet();
            adapter.Fill(results, "Result");

            if (results.Tables[0].Rows.Count > 0)
            {
                return results.Tables[0].Rows[0];
            }

            return null;
        }

        // GET: Index
        [Authorize]
        public ActionResult Index()
        {
            var authToken = CreateAuthToken();

            if (authToken != null)
            {
                ViewBag.userId = authToken["UserId"];
                ViewBag.HashToken = authToken["HashToken"];
                ViewBag.TokenExpireDate = authToken["ExpireDate"];
            }
            else
            {
                ViewBag.userId = User.Identity.GetUserId();
                ViewBag.HashToken = "unknown";
                ViewBag.TokenExpireDate = "unknown";
            }

            return View();
        }

        [Authorize]
        public ActionResult HttpClientMode(string SampleParameter)
        {
            var authToken = CreateAuthToken();

            if (authToken == null)
            {
                throw new Exception("No Authentication Token was generated");
            }
            else
            {
                var accessTokenPlainTextBytes = System.Text.Encoding.UTF8.GetBytes(
                    String.Format("{0}:{1}",
                        ConfigurationManager.AppSettings["SSRSAccount"],
                        ConfigurationManager.AppSettings["SSRSPassword"]
                        )
                    );

                string accessToken = System.Convert.ToBase64String(accessTokenPlainTextBytes);

                var url = String.Format(
                    ConfigurationManager.AppSettings["SSRSReportURL"] + "&rs:Command=Render&UserId={0}&HashToken={1}",
                    authToken["UserId"],
                    authToken["HashToken"]
                    );

                if (!String.IsNullOrEmpty(SampleParameter))
                {
                    url += "&SampleParameter=" + SampleParameter;
                }

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + accessToken);
                
                System.Threading.Tasks.Task<string> response = client.GetStringAsync(url);
                response.Wait();

                ViewBag.ReportHtml = response;

                return View();
            }
        }
    }
}