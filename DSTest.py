# Please install OpenAI SDK first: `pip3 install openai`

from openai import OpenAI

client = OpenAI(api_key="sk-493cc716c17e4860a82d4e3c633493d0", base_url="https://api.deepseek.com")

print("开始")
response = client.chat.completions.create(
    model="deepseek-chat",
    messages=[
        {"role": "system", "content": "介绍一下量化"}
    ],
    stream=False
)

print(response.choices[0].message.content)