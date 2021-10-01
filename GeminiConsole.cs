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

namespace GeminiClient
{
  class GeminiConsole
  {
    static void Main(string[] args)
    {
      try
      {
        if (args.Length == 0 || args[0] == "--help")
        {
          Console.WriteLine("Gemini user agent");
          Console.WriteLine("Usage:");
          Console.WriteLine("\tgemini url");
          Console.WriteLine("\tGets a gemini document from the provided URL.");
          return;
        }

        using(var client = new GeminiClient())
        {
          // Load the certificate
          var certificate = LoadX509Certificate();

          var uri = new Uri(args[0]);

          // Fetch the URL
          var response = client.Fetch(uri, certificate);

          Console.WriteLine(response);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("ERROR: " + ex.Message);
        if (ex.InnerException != null)
          Console.WriteLine("INNER ERROR: " + ex.InnerException.Message);
      }
    }

    static X509Certificate LoadX509Certificate(string certpath = "GeminiClient.crt")
    {
      return new X509Certificate2(certpath);
    }
  }
}
