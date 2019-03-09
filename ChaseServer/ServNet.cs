﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace ChaseServer
{
    class ServNet
    {
        // 常量
        public const int maxConn = 50;
        public static ServNet instance;

        public Socket listenfd;
        public Conn[] conns;

        // 协议
        public ProtocolBase proto;
        // 主定时器
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        // 心跳时间
        public long heartBeatTime = 180;

        // 消息分发
        public HandleConnMsg handleConnMsg = new HandleConnMsg();
        public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent();
        public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();

        public void HandleMainTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            HeartBeat();
            timer.Start();
        }

        public void HeartBeat()
        {
            //Console.WriteLine("[ 主定时器执行 ]");
            long timeNow = Sys.GetTimeStamp();

            for (int i = 0; i < conns.Length; i++)
            {
                Conn conn = conns[i];
                if (conn == null || !conn.isUse) continue;

                if (conn.lastTickTime < timeNow - heartBeatTime)
                {
                    Console.WriteLine("[ 心跳引起断开连接 ]" + conn.GetAdress());
                    lock (conn)
                    {
                        conn.Close();
                    }
                }
            }
        }

        public ServNet()
        {
            instance = this;
        }

        public int NewIndex()
        {
            if (conns == null)
                return -1;
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                {
                    conns[i] = new Conn();
                    return i;
                }
                else if (conns[i].isUse == false)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Start(string host, int port)
        {
            // 定时器
            timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
            timer.AutoReset = false;
            timer.Enabled = true;

            conns = new Conn[maxConn];
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Bind
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);
            // Listen
            listenfd.Listen(maxConn);
            // Accept
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("[服务器]启动成功");
        }

        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();

                if (index < 0)
                {
                    socket.Close();
                    Console.WriteLine("[警告]连接已满");
                }
                else
                {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAdress();
                    Console.WriteLine("客户端连接 [" + adr + "] conn池ID: " + index);
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(),
                        SocketFlags.None, ReceiveCb, conn);
                }
                listenfd.BeginAccept(AcceptCb, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptCb 失败: " + e.Message);
            }
        }

        private void ReceiveCb(IAsyncResult ar)
        {
            Conn conn = (Conn)ar.AsyncState;
            lock (conn)
            {
                try
                {
                    int count = conn.socket.EndReceive(ar);
                    // 关闭信号
                    if (count <= 0)
                    {
                        Console.WriteLine("收到 [" + conn.GetAdress() + "] 断开连接");
                        conn.Close();
                        return;
                    }

                    conn.buffCount += count;
                    ProcessData(conn);
                    
                    // 继续接收
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(),
                        SocketFlags.None, ReceiveCb, conn);
                }
                catch (Exception)
                {
                    Console.WriteLine("收到 [" + conn.GetAdress() + "] 断开连接");
                    conn.Close();
                }
            }
        }

        private void ProcessData(Conn conn)
        {
            // 小于长度字节
            if (conn.buffCount < sizeof(Int32)) return;

            // 消息长度
            Array.Copy(conn.readBuff, conn.lenBytes, sizeof(Int32));
            conn.msgLength = BitConverter.ToInt32(conn.lenBytes, 0);

            // 消息未接收完全
            if (conn.buffCount < conn.msgLength + sizeof(Int32)) return;

            // 处理消息
            ProtocolBase protocol = proto.Decode(conn.readBuff, sizeof(Int32), conn.msgLength);
            HandleMsg(conn, protocol);

            // 清除已处理的消息
            int count = conn.buffCount - conn.msgLength - sizeof(Int32);
            Array.Copy(conn.readBuff, sizeof(Int32) + conn.msgLength, conn.readBuff, 0, count);
            conn.buffCount = count;
            if (conn.buffCount > 0)
            {
                ProcessData(conn);
            }
        }

        public void HandleMsg(Conn conn, ProtocolBase protoBase)
        {
            string name = protoBase.GetName();
            string methodName = "Msg" + name;
            // 连接协议分发
            if (conn.player == null || name == "HeatBeat" || name == "Logout")
            {
                MethodInfo mm = handleConnMsg.GetType().GetMethod(methodName);
                if (mm == null)
                {
                    string str = "[ 警告 ]HandleMsg 没有处理连接方法 ";
                    Console.WriteLine(str + methodName);
                    return;
                }
                object[] obj = new object[] { conn, protoBase };
                Console.WriteLine("[ 处理连接消息 ]" + conn.GetAdress() + " :" + name);
                mm.Invoke(handleConnMsg, obj);
            }
            else
            {
                MethodInfo mm = handlePlayerMsg.GetType().GetMethod(methodName);
                if (mm == null)
                {
                    string str = "[ 警告 ]HandleMsg 没有处理连接方法 ";
                    Console.WriteLine(str + methodName);
                    return;
                }
                Object[] obj = new Object[] { conn.player, protoBase };
                Console.WriteLine("[ 处理玩家消息 ]" + conn.player.id + " :" + name);
                mm.Invoke(handlePlayerMsg, obj);
            }
            // 回射
            //Send(conn, protoBase);
        }

        public void Send(Conn conn, ProtocolBase protocol)
        {
            byte[] bytes = protocol.Encode();
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] sendbuff = length.Concat(bytes).ToArray();
            try
            {
                conn.socket.BeginSend(sendbuff, 0, sendbuff.Length, SocketFlags.None, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("[ 发送消息 ]" + conn.GetAdress() + " : " + e.Message);
            }
        }

        public void Broadcast(ProtocolBase protocol)
        {
            for (int i = 0; i < conns.Length; i++)
            {
                Conn conn = conns[i];
                if (conn == null || !conn.isUse) continue;
                if (conn.player == null) continue;
                Send(conn, protocol);
            }
        }

        public void Close()
        {
            for (int i = 0; i < conns.Length; i++)
            {
                Conn conn = conns[i];
                if (conn == null || !conn.isUse) continue;
                lock(conn)
                {
                    conn.Close();
                }
            }
        }

        public void Print()
        {
            Console.WriteLine("===服务器登录信息===");
            for (int i = 0; i < conns.Length; i++)
            {
                Conn conn = conns[i];
                if (conn == null || !conn.isUse) continue;
                string str = "连接 [" + conn.GetAdress() + "] ";
                if (conn.player != null)
                {
                    str += " 玩家 id " + conn.player.id;
                }
                Console.WriteLine(str);
            }
        }
    }
}
