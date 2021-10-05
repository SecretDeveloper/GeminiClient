using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Linq;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace GeminiClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                // "Gemini Console Client", "A simple gemini protocol client"
                var cmd = new RootCommand("A simple gemini protocol client")
                {
                    new Argument<string>("url", "The gemini url to fetch"),
                    new Argument<string>("certificatePath", "The path to the X509 certificate to use"),
                    new Option<string>("--password", "The password for the X509 certificate"),
                    new Option("--verbose", "Show debug information")
                };
                cmd.Name = "Fetch";
                cmd.Handler = CommandHandler.Create<string, string, string, bool>(Run);

                cmd.Invoke(args);
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("INNER ERROR: " + ex.InnerException.Message);
            }
        }

        /// <summary>
        /// Retrieves content using the provided url and outputs the result to STDOUT.
        /// </summary>
        /// <param name="url">The url to fetch</param>
        /// <param name="certificatePath">The path to the X509 certificate to use. Gemini clients are require to provide a certificate. If a path is not provided then a certificate called 'GeminiCert.crt' in the local folder will be used if one is present.</param>
        /// <param name="password"></param>
        /// <param name="verbose"></param>
        private static void Run(string url, string certificatePath = "GeminiCert.crt", string password = "", bool verbose = false)
        {
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            if (verbose) Console.WriteLine("Debug: URL={0}, CertPath={1}, Password={2}, Verbose={3}", url, certificatePath, password, verbose);
            using (var client = new GeminiClient())
            {
                // Load the certificate
                var certificate = LoadX509Certificate(certificatePath, password);
                // Fetch the URL
                var response = client.Fetch(new Uri(url), certificate, verbose);
                Console.WriteLine(response.ResponseBody);
                if (verbose) Console.WriteLine("Debug: Remote server returned: {0} bytes", response.ResponseBody.Length);
            }

            timer.Stop();
            if (verbose) Console.WriteLine("Debug: Took: {0}ms", timer.ElapsedMilliseconds);
        }

        private static X509Certificate LoadX509Certificate(string certpath, string password)
        {
            if (certpath == "" || System.IO.File.Exists(certpath) == false)
                throw new ApplicationException("A valid path to an X509 certificate must be provided.");

            return new X509Certificate2(certpath, password);
        }
    }
}
