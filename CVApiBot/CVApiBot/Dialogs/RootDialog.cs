using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace CVApiBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private static readonly HttpClient client = new HttpClient();

        public string httpPost(string url, Object obj, Dictionary<String, String> customHeaders)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            var json = JsonConvert.SerializeObject(obj);

            var data = Encoding.ASCII.GetBytes(json);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            foreach (var item in customHeaders)
            {
                request.Headers[item.Key] = item.Value;
            }

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return responseString;
        }

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            if (activity.Attachments.Count > 0 && activity.Attachments[0].ContentType.StartsWith("image"))
            {
                var attachmentData = await client.GetByteArrayAsync(activity.Attachments[0].ContentUrl);

                string base64ImageRepresentation = Convert.ToBase64String(attachmentData);

                string url = "Your Image API URL";

                var obj = new { img = base64ImageRepresentation };

                var resUrl = httpPost(url, obj, new Dictionary<string, string> { }).Replace("\"", "");

                string url2 = "Your Computer Vision API URL";

                var obj2 = new { url = resUrl };

                var cusHeaders = new Dictionary<string, string> {
                    { "Ocp-Apim-Subscription-Key", "Your Computer Vision API Subscription Key" }
                };

                var response = httpPost(url2, obj2, cusHeaders);

                var jsonObj = JsonConvert.DeserializeObject<CVApiResponse>(response);

                await context.PostAsync(jsonObj.description.captions[0].text);
            }
            else
            {
                await context.PostAsync("Please upload an image");
            }

            context.Wait(MessageReceivedAsync);
        }
    }
}