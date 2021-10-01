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

  public class GeminiResponse
  {
    // Gemini spec: https://gemini.circumlunar.space/docs/specification.gmi
    //
    // Header specification is the following content to be returned:
    // <STATUS><SPACE><META><CR><LF>
    // with sizes:
    // <2char><1char><Max 1024 bytes><1char><1char>
    // Assuming 4 bytes per char we get:
    // 8b + 4b + 1024b + 4b + 4b = 1044 bytes as max header size... I think.
    const int MAX_HEADER_LENGTH = 1044;

    public int Status { get; set; }
    public GeminiStatusType StatusType { get; set; }
    public string Meta { get; set; }
    public string ResponseBody { get; set; }

    public GeminiResponse(byte[] responseHeader, string responseBody="") : this(System.Text.Encoding.UTF8.GetString(responseHeader), responseBody)
    {}

    public GeminiResponse(string responseHeader, string responseBody="")
    {
        var split = responseHeader.IndexOf(" ");
        Status = int.Parse(responseHeader.Substring(0, split));
        StatusType = (GeminiStatusType)(Status - Status%10); //  Holy McHack!!!
        Meta = responseHeader.Substring(split, responseHeader.Length-split).TrimRight()+"\r\n";
        ResponseBody = responseBody;
    }

    public override string ToString()
    {
        return Status + " " + Meta + ResponseBody;
    }
  }

  public enum GeminiStatusType
  {
      Input = 10,
      Success = 20,
      Redirect = 30,
      TemporaryFailure = 40,
      PermanentFailure = 50,
      ClientCertificateRequired = 60
  }

  public class GeminiClient:System.IDisposable
  {
    bool _disposed = false;

    const int DEFAULT_SERVER_PORT = 1965; // Date of the first manned Gemini mission, Gemini 3 launched in March 1965.
    const int DEFAULT_TIMEOUT = 10000; // 10 seconds.

    public GeminiResponse Fetch(Uri uri, X509Certificate certificate)
    {
      var host = uri.Host;
      var port = uri.Port;
      if (port == -1) port = DEFAULT_SERVER_PORT;  // use default port if one is not included in Uri.

      var callback = new RemoteCertificateValidationCallback(AlwaysAcceptCertificate);

      var certs = new X509CertificateCollection();
      certs.Add(certificate);

      using (var client = new TcpClient(host, port))
      using (var stream = new SslStream(client.GetStream(), false, callback, null))
      {
        stream.ReadTimeout = DEFAULT_TIMEOUT;
        stream.AuthenticateAsClient(host, certs, SslProtocols.Tls12, false);

        // Gemini request
        // <URL><CR><LF>
        // URL is UTF-8 encoded
        byte[] request = Encoding.UTF8.GetBytes(uri.AbsoluteUri + "\r\n");
        stream.Write(request);
        stream.Flush();

        // read the header
        byte[] buffer = new byte[2048];
        stream.Read(buffer);
        var response = new GeminiResponse(buffer);

        Console.WriteLine("Debug: Status={0}, StatusType={1}, Meta={2}", response.Status, response.StatusType, response.Meta);

        if(response.StatusType == GeminiStatusType.Success)
        {
          //read the body
          using (MemoryStream ms = new MemoryStream())
          {
            stream.CopyTo(ms);
            response.ResponseBody = System.Text.Encoding.UTF8.GetString(ms.ToArray()).Trim();
          }
        }
        return response;
      }
    }

    private bool AlwaysAcceptCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
      // Just return true and ignore certificate errors for now.
      return true;
    }

    public void Dispose()
    {
        // Dispose of unmanaged resources.
        Dispose(true);
        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // TODO: dispose managed state (managed objects).
        }
        _disposed = true;
    }
  }
}
