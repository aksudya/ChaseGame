using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

public class Connection
{
    const int BUFFER_SIZE = 1024;                           //常量
    private Socket socket;                                  //socket
    private byte[] readBuff = new byte[BUFFER_SIZE];        //buffer
    private int buffCount = 0;
    
    private Int32 msgLength = 0;
    private byte[] lenBytes = new byte[sizeof(Int32)];      //粘包分包

    public ProtocolBase proto;                              //协议

    public float lastTickTime = 0;
    public float heartBeatTime = 30;                        //心跳时间

    public MsgDistribution msgDist = new MsgDistribution(); //消息分发

    public enum Status
    {
        None,
        connected,
    };                                                      //状态
    public Status status = Status.None;
    
    public bool Connect(string host,int port)               //链接服务器
    {
        try
        {
            socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,ProtocolType.Tcp);        //socket

            socket.Connect(host, port);

            socket.BeginReceive(readBuff, buffCount, 
                BUFFER_SIZE - buffCount, SocketFlags.None, 
                ReceiveCb, readBuff);                       //接收消息

            Debug.Log("链接成功");
            status = Status.connected;
            return true;
        }
        catch(Exception e)
        {
            Debug.Log("链接失败:" + e.Message);
            return false;
        }
    }    

    public bool Close()
    {
        try
        {
            socket.Close();
            return true;
        }
        catch(Exception e)
        {
            Debug.Log("关闭失败:" + e.Message);
            return false;
        }
    }

    private void ReceiveCb(IAsyncResult ar)                 //接收回调
    {
        try
        {
            int count = socket.EndReceive(ar);
            buffCount = buffCount + count;
            ProcessData();                                  //处理数据
            socket.BeginReceive(readBuff, buffCount, 
                BUFFER_SIZE - buffCount, SocketFlags.None, 
                ReceiveCb, readBuff);                       //再次调用接收消息
            
        }
        catch (Exception e)
        {
            Debug.Log("receivecb失败:" + e.Message);
            status = Status.None;          
        }
    }

    private void ProcessData()                      //处理数据（粘包分包）
    {
        if (buffCount<sizeof(Int32))
        {
            return;
        }

        Array.Copy(readBuff, lenBytes, sizeof(Int32));
        msgLength = BitConverter.ToInt32(lenBytes, 0);
        if(buffCount<msgLength+sizeof(Int32))
        {
            return;
        }                                           //计算包体长度

        ProtocolBase protocol = proto.Decode(readBuff, 
            sizeof(Int32), msgLength);
        Debug.Log("收到消息" + proto.GetDesc());     //解码

        lock(msgDist.msgList)                       
        {
            msgDist.msgList.Add(protocol);
        }                                           //线程加锁

        int count = buffCount - msgLength - sizeof(Int32);
        Array.Copy(readBuff, sizeof(Int32) + msgLength, readBuff, 0, count);
        buffCount = count;                          //清除已处理的消息

        if (buffCount>0)
        {
            ProcessData();
        }                                           //如果还有数据，继续处理
    }

    public bool Send(ProtocolBase protocol)         //基础版本send
    {
        if (status!=Status.connected)
        {
            Debug.Log("[Connection] 还未连接");
            return true;
        }
        byte[] b = protocol.Encode();
        byte[] length = BitConverter.GetBytes(b.Length);

        byte[] sendbuff = length.Concat(b).ToArray();
        socket.Send(sendbuff);

        Debug.Log("发送消息:" + protocol.GetDesc());
        return true;
    }

    public bool Send(ProtocolBase protocol,string cbName,MsgDistribution.Delegate cb)   //消息监听，可指定协议
    {
        if (status!=Status.connected)
        {
            return false;
        }
        msgDist.AddOnceListener(cbName, cb);
        return Send(protocol);
    }

    public bool Send(ProtocolBase protocol,MsgDistribution.Delegate cb)     //消息监听
    {
        string cbName = protocol.GetName();
        return Send(protocol, cbName, cb);
    }

    public void Update()        //心跳机制
    {
        msgDist.Update();
        if (status==Status.connected)
        {
            if(Time.time-lastTickTime>heartBeatTime)
            {
                ProtocolBase protocol = NetMgr.GetHeatBeatProtocol();
                Send(protocol);
                lastTickTime = Time.time;
            }
        }

    }
}
