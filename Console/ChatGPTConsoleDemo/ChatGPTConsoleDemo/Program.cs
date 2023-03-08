using System;
using System.IO;
using System.Net;
using System.Text;

namespace ChatGPTConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(GetAnswer("海安"));
            Console.Read();
        }

        public static string GetAnswer(string question)
        {
            return PostJson("https://www.pmzhang.cn/api/generate", "{\"messages\":[{\"role\":\"user\",\"content\":\"" + question + "\"}]}");
        }

        public static string PostJson(string url, string postJsonData)
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
                Console.WriteLine("Post异常:" + ex.ToString());
            }
            return "";
        }
    }
}
