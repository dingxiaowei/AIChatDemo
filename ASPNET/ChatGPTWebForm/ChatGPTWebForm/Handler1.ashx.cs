using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;

namespace ChatGPTWebForm
{
    /// <summary>
    /// 离线消息
    /// </summary>
    public class MessageInfo
    {
        public MessageInfo(DateTime _MsgTime, ArraySegment<byte> _MsgContent)
        {
            MsgTime = _MsgTime;
            MsgContent = _MsgContent;
        }
        public DateTime MsgTime { get; set; }
        public ArraySegment<byte> MsgContent { get; set; }

    }


    /// <summary>
    /// Handler1 的摘要说明
    /// </summary>
    public class Handler1 : IHttpHandler
    {
        private static Dictionary<string, WebSocket> CONNECT_POOL = new Dictionary<string, WebSocket>();//用户连接池
        private static Dictionary<string, List<MessageInfo>> MESSAGE_POOL = new Dictionary<string, List<MessageInfo>>();//离线消息池


        public void ProcessRequest(HttpContext context)
        {

            if (context.IsWebSocketRequest)
            {
                context.Response.ContentType = "application/json";
                context.Response.Charset = "utf-8";
                context.AcceptWebSocketRequest(ProcessMsg);
            }
        }


        private async Task ProcessMsg(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;
            string user = context.QueryString["user"].ToString();

            try
            {
                #region 用户添加连接池
                //第一次open时，添加到连接池中
                if (!CONNECT_POOL.ContainsKey(user))
                {
                    CONNECT_POOL.Add(user, socket);//不存在，添加
                }
                else
                {
                    if (socket != CONNECT_POOL[user])//当前对象不一致，更新
                    {
                        CONNECT_POOL[user] = socket;
                    }
                }
                #endregion

                //#region 连线成功
                //for (int cp = 0; cp < CONNECT_POOL.Count; cp++)
                //{
                //    if (CONNECT_POOL.ElementAt(cp).Key != user)
                //    {
                //        string joinedmsg = "{"FROM":"" + user + "","event":"joined"}";
                //        ArraySegment<byte> joinedmsgbuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(joinedmsg));
                //        WebSocket destSocket = CONNECT_POOL.ElementAt(cp).Value;//目的客户端
                //        await destSocket.SendAsync(joinedmsgbuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                //    }
                //}
                //#endregion

                #region 离线消息处理
                if (MESSAGE_POOL.ContainsKey(user))
                {
                    List<MessageInfo> msgs = MESSAGE_POOL[user];
                    foreach (MessageInfo item in msgs)
                    {
                        await socket.SendAsync(item.MsgContent, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    MESSAGE_POOL.Remove(user);//移除离线消息
                }
                #endregion

                while (true)
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        ArraySegment<byte> wholemessage = new ArraySegment<byte>(new byte[10240]);


                        int i = 0;


                        WebSocketReceiveResult dresult;
                        do
                        {
                            //因为websocket每一次发送的数据会被tcp分包
                            //所以必须判断接收到的消息是否完整
                            //不完整就要继续接收并拼接数据包
                            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[2048]);
                            dresult = await socket.ReceiveAsync(buffer, CancellationToken.None);
                            string message1 = Encoding.UTF8.GetString(buffer.Array);
                            buffer.Array.CopyTo(wholemessage.Array, i);
                            i += 2048;
                        } while (false == dresult.EndOfMessage);

                        //string message = Encoding.UTF8.GetString(wholemessage.Array);
                        //message = message.Replace("", "").Trim();
                        //JavaScriptSerializer serializer = new JavaScriptSerializer();
                        //Dictionary<string, object> json = (Dictionary<string, object>)serializer.DeserializeObject(message);
                        //string target = (string)json.ElementAt(1).Value;

                        #region 消息处理（字符截取、消息转发）
                        try
                        {
                            #region 关闭Socket处理，删除连接池
                            if (socket.State != WebSocketState.Open)//连接关闭
                            {
                                if (CONNECT_POOL.ContainsKey(user)) CONNECT_POOL.Remove(user);//删除连接池
                                break;
                            }
                            #endregion

                            for (int cp = 0; cp < CONNECT_POOL.Count; cp++)
                            {
                                //if (CONNECT_POOL.ElementAt(cp).Key!=target)
                                //   {
                                WebSocket destSocket = CONNECT_POOL.ElementAt(cp).Value;//目的客户端
                                await destSocket.SendAsync(wholemessage, WebSocketMessageType.Text, true, CancellationToken.None);
                                //  }
                            }


                            //if (CONNECT_POOL.ContainsKey(descUser))//判断客户端是否在线
                            //{
                            //    WebSocket destSocket = CONNECT_POOL[descUser];//目的客户端
                            //    if (destSocket != null && destSocket.State == WebSocketState.Open)
                            //        await destSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                            //}
                            //else
                            //{
                            //    _ = Task.Run(() =>
                            //      {
                            //          if (!MESSAGE_POOL.ContainsKey(descUser))//将用户添加至离线消息池中
                            //            MESSAGE_POOL.Add(descUser, new List<MessageInfo>());
                            //          MESSAGE_POOL[descUser].Add(new MessageInfo(DateTime.Now, buffer));//添加离线消息
                            //    });
                            //}
                        }
                        catch (Exception exs)
                        {
                            //消息转发异常处理，本次消息忽略 继续监听接下来的消息
                        }
                        #endregion
                    }
                    else
                    {
                        if (CONNECT_POOL.ContainsKey(user)) CONNECT_POOL.Remove(user);//删除连接池                        
                        break;
                    }
                }//while end
            }
            catch (Exception ex)
            {
                //整体异常处理
                if (CONNECT_POOL.ContainsKey(user)) CONNECT_POOL.Remove(user);

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