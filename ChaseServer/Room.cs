using System.Collections.Generic;
using System.Threading;

namespace ChaseServer
{
    class Room
    {
        public enum State
        {
            Prepare, Fight, Load, Delete
        }
        public State state = State.Prepare;

        public int readyPlayers = 0;
        public int onloadPlayers;
        public Dictionary<string, Player> list = new Dictionary<string, Player>();

        public Timer delRoomTimer;
        public Room()
        {
            delRoomTimer = new Timer(DeleteRoom, null, RoomMgr.waitReadyTime, Timeout.Infinite);
        }

        public bool AddPlayer(Player player)
        {
            lock (list)
            {
                PlayerTempData tempData = player.tempData;
                tempData.room = this;
                tempData.state = PlayerTempData.State.Room;
                list.Add(player.id, player);
            }
            return true;
        }

        public bool DelPlayer(string id)
        {
            lock (list)
            {
                if (!list.ContainsKey(id))
                    return false;
                PlayerTempData tempData = list[id].tempData;
                tempData.state = PlayerTempData.State.None;
                tempData.room = null;
                list.Remove(id);
            }
            return true;
        }

        public void DeleteRoom(object state)
        {
            delRoomTimer.Dispose();
            delRoomTimer = null;
            lock (this)
            {
                if (this.state != State.Prepare ||
                    readyPlayers == RoomMgr.maxPlayers)
                    return;
                this.state = State.Delete;
                readyPlayers = 0;
                lock (RoomMgr.instance.list)
                {
                    RoomMgr.instance.list.Remove(this);
                }

                ProtocolBytes protocol = new ProtocolBytes();
                protocol.AddString("LeaveRoom");
                if (state == null)
                    protocol.AddInt(408);       // 房间超时
                else
                {
                    protocol.AddInt(410);       // 玩家退出
                    Player player = (Player)state;
                    protocol.AddString(player.id);
                }

                lock (list)
                {
                    foreach (Player player in list.Values)
                    {
                        player.tempData.room = null;
                        player.tempData.state = PlayerTempData.State.None;
                        player.Send(protocol);
                    }
                }
            }
        }

        //public void DeleteRoom(object state)
        //{
        //    delRoomTimer.Dispose();
        //    delRoomTimer = null;
        //    lock (this)
        //    {
        //        if (state != state.Prepare ||
        //            readyPlayers == RoomMgr.maxPlayers)
        //            return;
        //        state = state.Delete;
        //        readyPlayers = 0;
        //        lock (RoomMgr.instance.list)
        //        {
        //            RoomMgr.instance.list.Remove(this);
        //        }

        //        ProtocolBytes protocolQue = new ProtocolBytes();
        //        protocolQue.AddString("Rematch");
        //        ProtocolBytes protocolNon = new ProtocolBytes();
        //        protocolNon.AddString("Nomatch");

        //        lock (list)
        //        {
        //            foreach (Player player in list.Values)
        //            {
        //                player.tempData.room = null;
        //                if (player.tempData.isReady ||
        //                    state != null && state != player)
        //                {
        //                    player.tempData.state = PlayerTempData.state.Queue;
        //                    lock (RoomMgr.instance.queueLock)
        //                    {
        //                        if (player.tempData.isKiller)
        //                            RoomMgr.instance.killers.Add(player);
        //                        else
        //                            RoomMgr.instance.humans.Add(player);
        //                    }
        //                    player.Send(protocolQue);
        //                }
        //                else
        //                {
        //                    player.tempData.state = PlayerTempData.state.None;
        //                    player.Send(protocolNon);
        //                }
        //            }
        //        }
        //    }
        //}

        public void CheckLoad()
        {
            lock (this)
            {
                if (readyPlayers == RoomMgr.maxPlayers)
                {
                    delRoomTimer.Dispose();
                    delRoomTimer = null;
                    StartLoad();
                }
            }
        }

        public void StartLoad()
        {
            state = State.Load;
            onloadPlayers = 0;
            foreach (Player player in list.Values)
            {
                player.tempData.isOnload = false;
                player.sceneData = new SceneData();
            }

            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("StartLoad");
            protocol.AddInt(RoomMgr.random.Next(RoomMgr.maxMaps));
            Broadcast(protocol);
        }

        public void CheckFight()
        {
            if (onloadPlayers == RoomMgr.maxPlayers)
            {
                state = State.Fight;
                ProtocolBytes protocol = new ProtocolBytes();
                protocol.AddString("StartFight");
                Broadcast(protocol);

                delRoomTimer = new Timer(GameOver, 2, RoomMgr.gameTime, Timeout.Infinite);
            }
        }

        public void UpdateInfo(string id, float x, float y, int score = 0)
        {
            SceneData p = list[id].sceneData;
            if (p == null) return;

            p.x = x;
            p.y = y;
            p.score = score;
        }

        public void KilledHuman(Player player)
        {
            if (player.tempData.isKiller) return;
            if (!player.sceneData.isLived) return;
            player.sceneData.isLived = false;

            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("KilledHuman");
            protocolRet.AddString(player.id);
            Broadcast(protocolRet);

            CheckOver();
        }

        public void CheckOver()
        {
            int state = 1;
            foreach (Player p in list.Values)
            {
                if (p.tempData.isKiller)
                {
                    if (!p.sceneData.isLived)
                    {
                        state = 2;
                        break;
                    }
                }
                else
                {
                    if (p.sceneData.isLived)
                    {
                        state = 0;
                        break;
                    }
                }
            }
            if (state > 0)
                GameOver(state);
        }

        public void GameOver(object state)
        {
            delRoomTimer.Dispose();
            delRoomTimer = null;

            lock (this)
            {
                this.state = State.Delete;
                lock (RoomMgr.instance.list)
                {
                    RoomMgr.instance.list.Remove(this);
                }

                ProtocolBytes protocol = new ProtocolBytes();
                protocol.AddString("GameOver");
                protocol.AddInt((int)state);

                lock (list)
                {
                    foreach (Player p in list.Values)
                    {
                        p.tempData.room = null;
                        p.tempData.state = PlayerTempData.State.None;
                        p.Send(protocol);

                        if (p.tempData.isKiller)
                        {
                            if ((int)state == 1)
                            {
                                p.data.ScoreKiller++;
                            }
                            else
                            {
                                p.data.ScoreKiller--;
                            }
                            //DataMgr.instance.SavePlayer(p);
                        }
                        else
                        {
                            if ((int)state == 1)
                            {
                                p.data.ScoreHuman--;
                            }
                            else
                            {
                                p.data.ScoreHuman++;
                            }
                            //DataMgr.instance.SavePlayer(p);
                        }
                    }
                }
            }
        }

        public void SendPlayerList(Player player)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("GetList");
            foreach (Player p in list.Values)
            {
                protocol.AddString(p.id);
                protocol.AddFloat(p.sceneData.x);
                protocol.AddFloat(p.sceneData.y);
                //protocol.AddInt(p.sceneData.score);
            }
            player.Send(protocol);
        }

        public void Broadcast(ProtocolBase protocol)
        {
            foreach (Player player in list.Values)
            {
                player.Send(protocol);
            }
        }
    }
}
