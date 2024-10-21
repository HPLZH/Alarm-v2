using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alarm_v2
{
    public class RC
    {
        readonly HttpClient client = new();
        public bool StopFlag { get; set; } = false;
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);

        public Task StartGet(string uri)
        {
            return Task.Run(async () =>
            {
                string text = "";
                Console.Clear();
                while (!StopFlag)
                {
                    try
                    {
                        var response = await client.GetStringAsync(uri);
                        if (response.StartsWith(text))
                        {
                            Console.Write(response[text.Length..]);
                        }
                        else
                        {
                            Console.Clear();
                            Console.Write(response);
                        }
                        text = response;
                    }
                    catch (Exception ex)
                    {
                        string errtext = ex.Message + "\n" + ex.StackTrace;
                        if (errtext.StartsWith(text))
                        {
                            Console.Write(errtext[text.Length..]);
                        }
                        else
                        {
                            Console.Clear();
                            Console.Write(errtext);
                        }
                        text = errtext;
                    }
                    await Task.Delay(Interval);
                }
            });
        }

        public Task<HttpResponseMessage> Pass(string uri)
        {
            try
            {
                return client.PostAsync(uri, null);
            }
            catch
            {
                return Task.Run(() => new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
            }
        }

        public Task<HttpResponseMessage> Stop(string uri)
        {
            try
            {
                return client.DeleteAsync(uri);
            }
            catch
            {
                return Task.Run(() => new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
            }
        }
    }
}
