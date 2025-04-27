using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using NoteEditor.Model;

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
                var response = EditDataSerializer.Serialize();
                Debug.Log("发送数据：" + response);
                Send(response);
            }
            else
            {
                // 如果type不是getchart，返回type键为null的json
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
        // 设置WebSocket服务器的端口
        wssv = new WebSocketServer(4649);

        // 添加服务
        wssv.AddWebSocketService<SimCrySocketServer>("/SimCrySocket");

        // 开始监听
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
