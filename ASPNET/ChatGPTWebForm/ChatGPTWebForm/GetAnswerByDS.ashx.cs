using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChatGPTWebForm
{

    // 定义请求数据结构 
    class ChatRequest
    {
        public string model { get; set; } = "deepseek-chat";
        public Message[] messages { get; set; }
        public bool stream { get; set; } = false;
    }

    class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    // 定义响应数据结构 
    class ChatResponse
    {
        public Choice[] choices { get; set; }
    }

    class Choice
    {
        public Message message { get; set; }
    }
    /// <summary>
    /// GetAnswerByDS 的摘要说明
    /// </summary>
    public class GetAnswerByDS : HttpTaskAsyncHandler
    {

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            // 处理请求
            context.Response.ContentType = "text/plain";
            var question = context.Request["question"];
            if (string.IsNullOrEmpty(question))
            {
                context.Response.Write("error:请传入问题");
            }
            else
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://api.deepseek.com");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer sk-493cc716c17e4860a82d4e3c633493d0");

                var request = new ChatRequest
                {
                    messages = new[]
                    {
                new Message
                {
                    role = "system",
                    content = question
                }
            }
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/v1/chat/completions", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ChatResponse>(responseString);
                    context.Response.Write(result?.choices?[0].message.content);
                }
                else
                {
                    context.Response.Write($"error:请求失败: {response.StatusCode}");
                }
            }
        }

        public override bool IsReusable
        {
            get { return true; }
        }
    }
}