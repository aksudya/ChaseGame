using System;
using System.Net.Sockets;

namespace ChaseServer
{
    class Conn
    {
        // 常量
        public const int BUFFER_SIZE = 1024;
      
        public Socket socket;
        public bool isUse = false;
        public byte[] readBuff = new byte[BUFFER_SIZE];
        public int buffCount = 0;

        // 粘包分包
        public byte[] lenBytes = new byte[sizeof(UInt32)];
        public Int32 msgLength = 0;
        // 心跳时间
        public long lastTickTime = long.MinValue;
        // 对应的Player
        public Player player;

        public Conn()
        {
        }

        public void Init(Socket socket)
        {
            this.socket = socket;
            isUse = true;
            buffCount = 0;
            // 心跳处理
            lastTickTime = Sys.GetTimeStamp();
        }

        public int BuffRemain()
        {
            return BUFFER_SIZE - buffCount;
        }

        public string GetAdress()
        {
            if (isUse)
                return socket.RemoteEndPoint.ToString();
            else
                return "无法获取地址";
        }

        public void Close()
        {
            if (!isUse)
                return;
            if (player != null)
            {
                player.Logout();
                return;
            }
            Console.WriteLine("[断开连接]" + GetAdress());
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            isUse = false;
        }

        public void Send(ProtocolBase protocol)
        {
            ServNet.instance.Send(this, protocol);
        }
    }
}
