using System;

namespace ChaseServer
{
    partial class HandlePlayerMsg
    {
        public void MsgEnterKiller(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("EnterKiller");
            lock (RoomMgr.instance.queueLock)
            {
                if (player.tempData.state != PlayerTempData.State.None)
                {
                    Console.WriteLine("[MsgEnterKiller] 状态不为None，无法加入");
                    protocol.AddInt(417);
                    player.Send(protocol);
                    return;
                }

                player.tempData.state = PlayerTempData.State.Queue;
                player.tempData.isKiller = true;
                RoomMgr.instance.killers.Add(player);
            }
            protocol.AddInt(200);
            player.Send(protocol);

            RoomMgr.instance.TestMatch();
        }

        public void MsgEnterHuman(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("EnterHuman");
            lock (RoomMgr.instance.queueLock)
            {
                if (player.tempData.state != PlayerTempData.State.None)
                {
                    Console.WriteLine("[MsgEnterHuman] 状态不为None，无法加入");
                    protocol.AddInt(417);
                    player.Send(protocol);
                    return;
                }

                player.tempData.state = PlayerTempData.State.Queue;
                player.tempData.isKiller = false;
                RoomMgr.instance.humans.Add(player);
            }
            protocol.AddInt(200);
            player.Send(protocol);

            RoomMgr.instance.TestMatch();
        }

        public void MsgLeaveQueue(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("LeaveQueue");
            bool flag;
            lock (RoomMgr.instance.queueLock)
            {
                if (player.tempData.state != PlayerTempData.State.Queue)
                {
                    Console.WriteLine("[MsgLeaveQueue] 已不在队列，无法退出");
                    protocol.AddInt(417);
                    player.Send(protocol);
                    return;
                }

                player.tempData.state = PlayerTempData.State.None;
                if (player.tempData.isKiller)
                    flag = RoomMgr.instance.killers.Remove(player);
                else
                    flag = RoomMgr.instance.humans.Remove(player);
            }
            if (!flag)
            {
                Console.WriteLine("[MsgLeaveQueue] Remove失败，无法退出");
                protocol.AddInt(500);
                player.Send(protocol);
                return;
            }

            protocol.AddInt(200);
            player.Send(protocol);
        }

        public void MsgReadyFight(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("ReadyFight");
            protocol.AddString(player.id);

            if (player.tempData.state != PlayerTempData.State.Room)
            {
                Console.WriteLine("[MsgReadyFight] 玩家状态错误");
                protocol.AddInt(417);
                player.Send(protocol);
                return;
            }
            if (player.tempData.room == null ||
                player.tempData.room.delRoomTimer == null)
            {
                Console.WriteLine("[MsgReadyFight] 房间已删除，无法准备");
                protocol.AddInt(418);
                player.Send(protocol);
                return;
            }

            protocol.AddInt(200);
            player.tempData.room.Broadcast(protocol);
            //player.Send(protocol);
            if (!player.tempData.isReady)
            {
                player.tempData.isReady = true;
                player.tempData.room.readyPlayers++;
                player.tempData.room.CheckLoad();
            }
        }

        public void MsgLeaveRoom(Player player, ProtocolBase protoBase)
        {
            //if (player.tempData.room == null) return;
            //Room room = player.tempData.room;
            //room.DelPlayer(player.id);
            //if (room.list.Count == 0)
            //{
            //    lock (RoomMgr.instance.list)
            //    {
            //        RoomMgr.instance.list.Remove(room);
            //    }
            //}
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("LeaveRoom");

            if (player.tempData.state != PlayerTempData.State.Room)
            {
                Console.WriteLine("[MsgLeaveRoom] 玩家状态错误");
                protocol.AddInt(417);
                player.Send(protocol);
                return;
            }
            if (player.tempData.room == null ||
                player.tempData.room.delRoomTimer == null)
            {
                Console.WriteLine("[MsgLeaveRoom] 房间已删除，无需离开");
                protocol.AddInt(418);
                player.Send(protocol);
                return;
            }

            lock (player.tempData.room)
            {
                player.tempData.room.DeleteRoom(player);
            }

            //protocol.AddInt(200);
            //player.Send(protocol);
        }

        public void MsgMapOnload(Player player, ProtocolBase protoBase)
        {
            if (player.tempData.isOnload) return;
            player.tempData.isOnload = true;
            Room room = player.tempData.room;
            room.onloadPlayers++;
            room.CheckFight();
        }
    }
}
