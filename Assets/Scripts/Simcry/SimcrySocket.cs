using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using NoteEditor.Model;
using NoteEditor.Presenter;
public class SimCrySocketServer : WebSocketBehavior
{
    protected override void OnOpen()
    {
        Debug.Log("连接成功");
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        // 接收到json消息时
        if (e.IsText)
        {
            var message = e.Data;
            var jsonMessage = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

            // 如果type键的值为getchart
            if (jsonMessage.ContainsKey("type") && jsonMessage["type"] == "getchart")
            {
                Debug.Log("接收到getchart请求");
                var response = EditDataSerializer.Serializesocket();
                Debug.Log("成功发送Chart数据");
                Send(response);
            }
            else if (jsonMessage.ContainsKey("type") && jsonMessage["type"] == "getinfo")
            {
                Debug.Log("接收到getinfo请求");
                var response = JsonConvert.SerializeObject(new { time = Audio.TimeSamples.Value / 44100f });
                Debug.Log("成功发送info数据");
                Send(response);
            }
            else
            {
                var response = JsonConvert.SerializeObject(new { type = (string)null });
                Send(response);
            }
        }
    }
}

public class SimcrySocket : MonoBehaviour
{
    private WebSocketServer wssv;

    void Start()
    {
        wssv = new WebSocketServer(4649);

        wssv.AddWebSocketService<SimCrySocketServer>("/SimCrySocket");

        wssv.Start();
        Debug.Log("WebSocket服务器已启动");
    }

    void OnDestroy()
    {
        // 停止服务器
        if (wssv != null)
        {
            wssv.Stop();
        }
    }
}
