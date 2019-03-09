using System;

namespace ChaseServer
{
    class HandleConnMsg
    {
        public void MsgHeatBeat(Conn conn, ProtocolBase protoBase)
        {
            conn.lastTickTime = Sys.GetTimeStamp();
            Console.WriteLine("[ 更新心跳时间 ]" + conn.GetAdress());
        }

        public void MsgRegister(Conn conn, ProtocolBase protoBase)
        {
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            string id = protocol.GetString(start, ref start);
            string pw = protocol.GetString(start, ref start);
            string strFormat = "[ 收到注册协议 ]" + conn.GetAdress();
            Console.WriteLine(strFormat + " 用户名: " + id + " 密码: " + pw);

            protocol = new ProtocolBytes();
            protocol.AddString("Register");

            protocol.AddInt(DataMgr.instance.Register(id, pw));

            DataMgr.instance.CreatePlayer(id);
            conn.Send(protocol);
        }

        public void MsgLogin(Conn conn, ProtocolBase protoBase)
        {
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            string id = protocol.GetString(start, ref start);
            string pw = protocol.GetString(start, ref start);
            string strFormat = "[ 收到登录协议 ]" + conn.GetAdress();
            Console.WriteLine(strFormat + " 用户名: " + id + " 密码: " + pw);

            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("Login");
            int ckstate = DataMgr.instance.CheckPassWord(id, pw);
            if (ckstate != 200)
            {
                protocolRet.AddInt(ckstate);
                conn.Send(protocolRet);
                return;
            }

            ProtocolBytes protocolLogout = new ProtocolBytes();
            protocolLogout.AddString("Logout");
            int kfstate = Player.KickOff(id, protocolLogout);
            if (kfstate != 200)
            {
                protocolRet.AddInt(kfstate);
                conn.Send(protocolRet);
                return;
            }

            PlayerData playerData = DataMgr.instance.GetPlayerData(id);
            if (playerData == null)
            {
                protocolRet.AddInt(502);
                conn.Send(protocolRet);
                return;
            }
            conn.player = new Player(id, conn);
            conn.player.data = playerData;

            ServNet.instance.handlePlayerEvent.OnLogin(conn.player);

            protocolRet.AddInt(200);
            conn.Send(protocolRet);
        }

        public void MsgLogout(Conn conn, ProtocolBase protoBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("Logout");
            protocol.AddInt(200);
            conn.Send(protocol);
            if (conn.player == null)
            {
                conn.Close();
            }
            else
            {
                conn.player.Logout();
            }
        }
    }
}
