using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;

namespace Interceptor
{
    static class Program
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);
        
        private delegate bool HandlerRoutine();

        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private static Models.Interceptor _interceptor;

        static void Main(string[] args)
        {
            try
            {
                // Handle the closing of the application
                SetConsoleCtrlHandler(Close, true);

                FiddlerApplication.Startup(6575, FiddlerCoreStartupFlags.Default);

                var oAllSessions = new List<Session>();

                var handler = new ConfigurationHandler.ConfigurationHandler();
                _interceptor = handler.LoadConfig();

                FiddlerApplication.BeforeRequest += delegate(Session oS)
                {
                    switch (oS.oRequest.headers.HTTPMethod)
                    {
                        case "OPTIONS":
                            oS.utilCreateResponseAndBypassServer();
                            oS.responseCode = 200;
                            oS.oResponse.headers.HTTPResponseStatus = "200 OK";
                            oS.oResponse.headers.Add("Access-Control-Allow-Headers", "accept, x-requested-with, X-Requested-With");
                            oS.oResponse.headers.Add("Access-Control-Allow-Methods", "GET");
                            oS.oResponse["Access-Control-Allow-Origin"] = oS.oRequest.headers["Origin"];
                            oS.oResponse["Access-Control-Allow-Credentials"] = "true";
                            break;
                        case "GET":
                            Models.InterceptorService service = TestUrl(oS);

                            if (service != null)
                            {
                                BuildHeader(oS);
                                string response;
                                if (service.Response.Type != null && service.Response.Type.ToUpper() == "FILE")
                                {
                                    string path =
                                        System.Configuration.ConfigurationManager.AppSettings["DefaultJsonFileLocation"] +
                                        @"\" + service.Response.Value;
                                    response = LoadJsonFile(path);
                                }
                                else
                                {
                                    response = service.Response.Value;
                                }

                                oS.utilSetResponseBody(response);
                            }
                            break;
                    }
                };

                FiddlerApplication.BeforeResponse += delegate(Session oS)
                {
                    if (System.Configuration.ConfigurationManager.AppSettings["LogRequests"].ToLower() == "true")
                    {
                        if (TestUrl(oS) == null)
                        {
                            string urlFilter =
                                System.Configuration.ConfigurationManager.AppSettings["LogRequestsFilter"];
                            urlFilter = urlFilter.Replace("*", ".*");
                            if (Regex.IsMatch(oS.url, urlFilter))
                            {
                                Logger.Info("Matched URL: " + oS.url);
                                Logger.Info("Matched Header: " + oS.oResponse.headers);
                                Logger.Info("Matched Json: " + oS.GetResponseBodyAsString());

                                //Save File

                                var jsonFileName = oS.url.Split('/').Reverse().ToList()[0];
                                oS.SaveResponseBody(
                                    System.Configuration.ConfigurationManager.AppSettings["DefaultJsonFileLocation"] +
                                    @"\" + jsonFileName + ".json");
                            }
                        }
                    }
                };

                FiddlerApplication.AfterSessionComplete += delegate
                                                           {
                    //Console.WriteLine("Finished session:\t" + oS.fullUrl); 
                    Console.Title = ("Session list contains: " + oAllSessions.Count + " sessions");
                };


                bool bDone = false;
                do
                {
                    Console.WriteLine("\nEnter a command [Q=Quit]:");
                    Console.Write(">");
                    ConsoleKeyInfo cki = Console.ReadKey();
                    Console.WriteLine();

                    switch (cki.KeyChar)
                    {
                        case 'q':
                            Close();
                            bDone = true;
                            break;
                    }
                } while (!bDone);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                Close();
            }
        }

        private static Models.InterceptorService TestUrl(Session oS)
        {
            var filteredServices = _interceptor.Services.
                Where(x => x.HttpMethod == oS.oRequest.headers.HTTPMethod && IsMatch(x.Url, oS.url)).ToList();

            switch (filteredServices.Count)
            {
                case 0:
                    Logger.Debug("Unable to match URL: " + oS.url);
                    break;
                case 1:
                    return filteredServices[0];
                default:
                    Logger.Error("Too many Services in configuration file match the request.");
                    break;
            }

            // If there is any errors found just return null
            return null;
        }

        private static bool IsMatch(string serviceUrl, string httpUrl)
        {
            var inputString = serviceUrl.Replace("*", ".*");
            if (Regex.IsMatch(httpUrl, inputString))
            {
                return true;
            }

            return false;
        }

        private static void BuildHeader(Session oS)
        {
            oS.utilCreateResponseAndBypassServer();
            oS.responseCode = 200;
            oS.oResponse.headers.HTTPResponseStatus = "200 OK";
            oS.oResponse["Content-Type"] = "application/json; charset=utf-8";
            oS.oResponse["Access-Control-Allow-Credentials"] = "true";
            oS.oResponse["Access-Control-Allow-Origin"] = oS.oRequest.headers["Origin"];
        }

        private static bool Close()
        {
            URLMonInterop.ResetProxyInProcessToDefault();
            FiddlerApplication.Shutdown();

            return true;
        }

        private static string LoadJsonFile(string path)
        {
            var sb = new StringBuilder();
            using (var sr = new StreamReader(path, Encoding.GetEncoding("iso-8859-1")))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }
    }
}
