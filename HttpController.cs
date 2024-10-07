using System.Net;
using System.Text;
using static Alarm_v2.AppConfig;

namespace Alarm_v2
{
    public class HttpController(Controller controller, IO io)
    {
        public readonly HttpListener listener = new();
        
        readonly Controller controller = controller;
        readonly IO io = io;

        public Task Start()
        {
            return Task.Run(() =>
            {
                listener.Start();
                while (listener.IsListening)
                {
                    var context = listener.GetContext();
                    Task.Run(() => { HttpTask(context); });
                }
            });
        }

        void HttpTask(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            if(request.HttpMethod == "GET")
            {
                response.StatusCode = 200;
                response.ContentType = "text/plain;charset=UTF-8";
                response.Close(Encoding.UTF8.GetBytes(io.GetContent()), false);
            }
            else if(request.HttpMethod == "POST")
            {
                controller.Pass();
                response.StatusCode = 204;
                response.Close();
            }
            else if (request.HttpMethod == "DELETE")
            {
                controller.Stop();
                response.StatusCode = 204;
                response.Close();
                Exit(0);
            }
            else
            {
                response.StatusCode = 405;
                response.AppendHeader("Allow", "GET, POST");
                response.Close();
            }
        }
    }
}
