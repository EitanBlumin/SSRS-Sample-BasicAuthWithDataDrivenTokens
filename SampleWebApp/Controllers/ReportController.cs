﻿using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;

namespace SampleWebApp.Controllers
{
    public class ReportController : Controller
    {
        private string GetReportUrl(string UserId, string HashToken, string ReportName = "SampleReportWithDataDrivenToken", string SampleParameter = "", string Format = "")
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            if (!String.IsNullOrEmpty(SampleParameter))
            {
                arguments.Add("SampleParameter", SampleParameter);
            }
            if (!String.IsNullOrEmpty(Format))
            {
                arguments.Add("rs:Format", Format);
            }

            return GetReportUrl(UserId, HashToken, ReportName, arguments);
        }

        /// <summary>
        /// Generic function for generating a report URL based on report name, data-driven token, and additional report parameters.
        /// The function uses the SSRSReportBaseURL configuration as the base URL (i.e. prefix).
        /// </summary>
        /// <param name="ReportName">The name of the report as it was deployed to the Report Server. Default: SampleReportWithDataDrivenToken</param>
        /// <param name="UserId">The user identifier to use with data-driven tokens.</param>
        /// <param name="HashToken">The random string to use with data-driven tokens.</param>
        /// <param name="args">List of additional parameters to pass on to the report (key/value dictionary).</param>
        /// <returns>Fully qualified URL path of the report.</returns>
        private string GetReportUrl(string UserId, string HashToken, string ReportName, Dictionary<string,string> args)
        {
            if (string.IsNullOrEmpty(ReportName))
            {
                ReportName = "SampleReportWithDataDrivenToken";
            }

            var url = String.Format(
                ConfigurationManager.AppSettings["SSRSReportBaseURL"] + "/ReportServer/Pages/ReportViewer.aspx?"
                + ConfigurationManager.AppSettings["SSRSReportsBasePath"] + "{0}&rs:Command=Render&rc:Toolbar=false&UserId={1}&HashToken={2}",
                ReportName,
                UserId,
                HashToken
                );

            foreach (string paramKey in args.Keys)
            {
                if (!String.IsNullOrEmpty(args[paramKey]))
                {
                    url += String.Format("&{0}={1}", paramKey, args[paramKey]);
                }
            }

            return url;
        }

        private string GetBasicAuthenticationAccessToken()
        {
            return System.Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(
                        String.Format("{0}:{1}",
                            ConfigurationManager.AppSettings["SSRSAccount"],
                            ConfigurationManager.AppSettings["SSRSPassword"]
                        )
                    )
                );
        }

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

        //[Authorize(Roles = "DOMAIN\Role1,DOMAIN\Role2")]
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
        
        //[Authorize(Roles = "DOMAIN\Role1,DOMAIN\Role2")]
        [Authorize]
        public ActionResult HttpClientMode(string id, string format)
        {
            ViewBag.ErrorData = new List<string>();

            try
            {
                var authToken = CreateAuthToken();

                if (authToken == null)
                {
                    throw new Exception("No Authorization Token was generated");
                }
                else
                {
                    string url = GetReportUrl(authToken["UserId"].ToString(), authToken["HashToken"].ToString(), default, id, format);
                    string accessToken = GetBasicAuthenticationAccessToken();
                    string authType = "Basic"; // you can also change this to "NTLM"

                    CredentialCache credentialCache = new CredentialCache();
                    credentialCache.Add(new Uri(ConfigurationManager.AppSettings["SSRSReportBaseURL"])
                                            , authType, new NetworkCredential(
                                                            ConfigurationManager.AppSettings["SSRSAccount"],
                                                            ConfigurationManager.AppSettings["SSRSPassword"]
                                                            )
                                            );

                    using (HttpClientHandler handler = new HttpClientHandler() { Credentials = credentialCache })
                    {
                        HttpClient client = new HttpClient(handler);
                        client.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", "*");

                        ViewBag.reportUrl = url;
                        ViewBag.accessToken = accessToken;
                        System.Threading.Tasks.Task<HttpResponseMessage> response = client.GetAsync(url);

                        try
                        {
                            response.Wait();

                            if (response.Result == null || response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                            {
                                if (response.Result == null)
                                    throw new Exception("Null response");
                                else
                                    throw new Exception(String.Format("Report Server returned an error ({0})", response.Result.StatusCode));
                            }

                            HttpContent content = response.Result.Content;
                            System.Threading.Tasks.Task<string> result = content.ReadAsStringAsync();

                            result.Wait();

                            ViewBag.ReportHtml = result.Result;
                        }
                        catch (Exception e)
                        {
                            ViewBag.ErrorTitle = "Report Server Error!";

                            if (response != null && response.Result != null)
                            {
                                ViewBag.ErrorTitle += String.Format(" (status code {0} - {1})", (int)response.Result.StatusCode, response.Result.StatusCode);
                            }

                            ViewBag.errorMessage = e.Message;
                            var ErrorData = new List<string>();

                            Exception innerEx = e.InnerException;
                            int depth = 1;

                            while (innerEx != null && depth < 5)
                            {
                                ErrorData.Add(innerEx.Message);
                                innerEx = innerEx.InnerException;
                                depth++;
                            }

                            ViewBag.ErrorData = ErrorData;

                            ViewBag.ErrorStackTrace = e.StackTrace;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.ErrorTitle = "Error While Setting Up Authorization!";

                ViewBag.errorMessage = e.Message;
                var ErrorData = new List<string>();

                Exception innerEx = e.InnerException;
                int depth = 1;

                while (innerEx != null && depth < 5)
                {
                    ErrorData.Add(innerEx.Message);
                    innerEx = innerEx.InnerException;
                    depth++;
                }

                ViewBag.ErrorData = ErrorData;

                ViewBag.ErrorStackTrace = e.StackTrace;
            }
            return View();
        }

        //[Authorize(Roles = "DOMAIN\Role1,DOMAIN\Role2")]
        [Authorize]
        public ActionResult JavaScriptMode(string id, string format)
        {
            ViewBag.ErrorData = new List<string>();

            try
            {
                var authToken = CreateAuthToken();

                if (authToken == null)
                {
                    throw new Exception("No Authorization Token was generated");
                }
                else
                {
                    ViewBag.reportUrl = GetReportUrl(authToken["UserId"].ToString(), authToken["HashToken"].ToString(), default, id, format);
                    ViewBag.accessToken = GetBasicAuthenticationAccessToken();
                }
            }
            catch (Exception e)
            {
                ViewBag.ErrorTitle = "Error While Setting Up Authorization!";

                ViewBag.errorMessage = e.Message;
                var ErrorData = new List<string>();

                Exception innerEx = e.InnerException;
                int depth = 1;

                while (innerEx != null && depth < 5)
                {
                    ErrorData.Add(innerEx.Message);
                    innerEx = innerEx.InnerException;
                    depth++;
                }

                ViewBag.ErrorData = ErrorData;

                ViewBag.ErrorStackTrace = e.StackTrace;
            }
            return View();
        }
    }
}