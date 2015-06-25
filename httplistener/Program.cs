using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using Jil;
using Dapper;
using System.Net;

namespace httplistener
{
  class Program
  {
    static void Main(string[] args)
    {

      Listen().Wait();

    }

    static async Task Listen()
    {
      var listener = new HttpListener();
      listener.Prefixes.Add("http://*:8080/");
      listener.Start();

      while (true)
      {
        var context = await listener.GetContextAsync();
        Serve(context);

      }
    }

    static async Task Serve(HttpListenerContext context)
    {
      var request = context.Request;
      var response = context.Response;
      string responseString = null;

      
      switch (request.Url.LocalPath)
      {
        case "/fortunes":
          responseString = Fortunes(request,response);
          break;

      }

      WriteResponse(response, responseString);


    }

    private static void WriteResponse(HttpListenerResponse response, String responseString)
    {
      response.ContentType = response.ContentType + "; charset=utf-8";
      byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
      response.ContentLength64 = buffer.Length;
      try
      {
        response.OutputStream.Write(buffer, 0, buffer.Length);
      }
      catch (Exception e)
      {
        // Ignore I/O errors
      }
    }

    private static string Fortunes(HttpListenerRequest request, HttpListenerResponse response)
    {
      List<Fortune> fortunes;

      using (var conn = new SqliteConnection("Data Source=fortunes.sqlite"))
      {
        conn.Open();
        fortunes = conn.Query<Fortune>("SELECT * FROM Fortune").ToList();

      }


      fortunes.Add(new Fortune { ID = 0, Message = "Additional fortune added at request time." });
      fortunes.Sort();

      response.ContentType = "text/html";

      const string header = "<!DOCTYPE html><html><head><title>Fortunes</title></head><body><table><tr><th>id</th><th>message</th></tr>";

      const string footer = "</table></body></html>";

      var body = string.Join("", fortunes.Select(x => "<tr><td>" + x.ID + "</td><td>" + x.Message + "</td></tr>"));

      return header + body + footer;

    }


  }

  public class Fortune : IComparable<Fortune>
  {
    public int ID { get; set; }
    public string Message { get; set; }

    public int CompareTo(Fortune other)
    {
      return Message.CompareTo(other.Message);
    }
  }
}
