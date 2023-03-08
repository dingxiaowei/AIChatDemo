using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace ChatGPTWebForm
{
    /// <summary>
    /// GetAnswer 的摘要说明
    /// </summary>
    public class GetAnswer : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            var question = context.Request["question"];
            if (string.IsNullOrEmpty(question))
            {
                context.Response.Write("error:请传入问题");
            }
            else
            {
                var answer = GetAnswerWithQuestion(question);
                context.Response.Write(answer);
            }
        }

        string GetAnswerWithQuestion(string question)
        {
            return PostJson("https://www.pmzhang.cn/api/generate", "{\"messages\":[{\"role\":\"user\",\"content\":\"" + question + "\"}]}");
        }

        string PostJson(string url, string postJsonData)
        {
            string result = "";
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = "text/plain;charset=UTF-8";
                req.Accept = "*/*";
                var data = Encoding.UTF8.GetBytes(postJsonData);
                req.ContentLength = data.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
                var resp = (HttpWebResponse)req.GetResponse();
                var stream = resp.GetResponseStream();

                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
                return result;
            }
            catch (Exception ex)
            {
                return $"Post异常:{ex}";
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}