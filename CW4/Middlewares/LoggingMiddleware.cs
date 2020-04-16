using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CW4.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            if (context.Request != null)
            {
                string path = context.Request.Path, method = context.Request.Method, queryString = context.Request.QueryString.ToString(), body = "";

                using (StreamReader streamReader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    body = await streamReader.ReadToEndAsync();
                    
                    context.Request.Body.Position = 0;
                }

                StringBuilder infoR = new StringBuilder();
                infoR.Append("Method: ").Append(method).Append("\n").Append("Path: ").Append(path).Append("\n").Append("Body:\n").Append(body).Append("\n").Append("Query_String: ").Append(queryString).Append("\n");

                using StreamWriter streamWriter = File.AppendText("infoLog.txt");
                streamWriter.Write(infoR);
            }

            await _next(context);
        }
    }
}
