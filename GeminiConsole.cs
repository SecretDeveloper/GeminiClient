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
              new Option<string>("--cert", "The X509 certificate to use"),
              new Option<string>("--password", "The password for the X509 certificate"),
              new Option("--verbose", "Show debug information"),
          };
          cmd.Name = "Gemini";
          cmd.Handler = CommandHandler.Create<string, string, string, bool>(Run);

          cmd.Invoke(args);
        }
        catch (Exception ex)
        {
          Console.WriteLine("ERROR: " + ex.Message);
          if (ex.InnerException != null)
            Console.WriteLine("INNER ERROR: " + ex.InnerException.Message);
        }
    }

    private static void Run(string url, string certificatePath="", string password="", bool verbose=false)
    {
        using(var client = new GeminiClient())
        {
          // Load the certificate
          var certificate = LoadX509Certificate();
          // Fetch the URL
          var response = client.Fetch(new Uri(url), certificate);
          Console.WriteLine(response);
        }
    }

    static X509Certificate LoadX509Certificate(string certpath = "GeminiClient.crt")
    {
      return new X509Certificate2(certpath);
    }
  }
}
