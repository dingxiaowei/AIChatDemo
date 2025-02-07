﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("开始");

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
                    content = "介绍一下C#"
                }
            }
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/v1/chat/completions", content);
        var responseString = await response.Content.ReadAsStringAsync();
        Console.WriteLine("准备打印");
        if (response.IsSuccessStatusCode)
        {
            var result = JsonConvert.DeserializeObject<ChatResponse>(responseString);
            Console.WriteLine(result?.choices?[0].message.content);
            Console.WriteLine("已经打印了");
        }
        else
        {
            Console.WriteLine($"请求失败: {response.StatusCode}");
            Console.WriteLine(responseString);
        }
        Console.Read();
    }
}