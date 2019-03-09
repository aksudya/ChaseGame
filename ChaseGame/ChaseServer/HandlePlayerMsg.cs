using System;

namespace ChaseServer
{
    partial class HandlePlayerMsg
    {
        public void MsgGetScore(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("GetScore");
            protocolRet.AddInt(player.data.ScoreHuman);
            protocolRet.AddInt(player.data.ScoreKiller);
            player.Send(protocolRet);
            Console.WriteLine("MsgGetScore " + player.id + " [" + player.data.ScoreHuman + " " + player.data.ScoreKiller + "] ");
        }

        public void MsgUpdateInfo(Player player, ProtocolBase protoBase)
        {
            Room room = player.tempData.room;
            if (room == null) return;
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            float x = protocol.GetFloat(start, ref start);
            float y = protocol.GetFloat(start, ref start);
            //int score = protocol.GetInt(start, ref start);
            room.UpdateInfo(player.id, x, y);

            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("UpdateInfo");
            protocolRet.AddString(player.id);
            protocolRet.AddFloat(x);
            protocolRet.AddFloat(y);
            //protocolRet.AddInt(score);
            room.Broadcast(protocolRet);
        }

        public void MsgKilledHuman(Player player, ProtocolBase protoBase)
        {
            if (!player.tempData.isKiller) return;
            Room room = player.tempData.room;
            if (room == null) return;

            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            string humanID = protocol.GetString(start, ref start);
            room.KilledHuman(room.list[humanID]);
        }
    }
}
