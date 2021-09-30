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
  class Program
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

        // Load the certificate
        var certificate = LoadX509Certificate();

        var uri = new Uri(args[0]);
        // Fetch the URL
        var response = Fetch(uri, certificate);

        Console.WriteLine(response);
      }
      catch (Exception ex)
      {
        Console.WriteLine("ERROR: " + ex.Message);
        if (ex.InnerException != null)
          Console.WriteLine("INNER ERROR: " + ex.InnerException.Message);
      }
    }

    static X509Certificate LoadX509CertificateWithPassword()
    {
      var password = "";
      Console.WriteLine("Enter certificate password:");
      password = Console.ReadLine();

      return new X509Certificate2(@"GeminiClient.crt", password);
    }

    static X509Certificate LoadX509Certificate()
    {
      return new X509Certificate2(@"GeminiClient.crt");
    }

    static string Fetch(Uri uri, X509Certificate certificate)
    {
      var host = uri.Host;
      var port = uri.Port;
      if (port == -1) port = 1965;

      var callback = new RemoteCertificateValidationCallback(AlwaysAcceptCertificate);

      var certs = new X509CertificateCollection();
      certs.Add(certificate);

      using (var client = new TcpClient(host, port))
      using (var stream = new SslStream(client.GetStream(), false, callback, null))
      {
        stream.ReadTimeout = 10 * 1000; // 10 seconds
        stream.AuthenticateAsClient(host, certs, SslProtocols.Tls12, false);

        byte[] request = Encoding.UTF8.GetBytes(uri.AbsoluteUri + "\r\n");

        stream.Write(request, 0, request.Count());
        stream.Flush();

        // Gemini spec: https://gemini.circumlunar.space/docs/specification.gmi
        //
        // Header specification is the following content to be returned:
        // <STATUS><SPACE><META><CR><LF>
        // with sizes:
        // <2char><1char><Max 1024 bytes><1char><1char>
        // Assuming 4 bytes per char we get:
        // 8b + 4b + 1024b + 4b + 4b = 1044 bytes as max header size... I think.
        const int MAX_HEADER_LENGTH = 1044;
        byte[] buffer = new byte[2048]; // Use a larger buffer so we can check actual read length -- could have been 1045 [shrug].

        // read the header
        var headerLength = stream.Read(buffer);
        if (headerLength > MAX_HEADER_LENGTH) return "ERROR: Response header exceeded maximum allowed length.";

        // Todo: Write the header if we are in some verbose mode.
        //Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer).Trim());

        //read the body
        using (MemoryStream ms = new MemoryStream())
        {
          stream.CopyTo(ms);
          var response = System.Text.Encoding.UTF8.GetString(ms.ToArray()).Trim();
          return response;
        }
      }
    }

    public static bool AlwaysAcceptCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
      // Just return true and ignore certificate errors for now.
      return true;
    }
  }
}
