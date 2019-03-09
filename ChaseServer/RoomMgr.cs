using System;
using System.Collections.Generic;
using System.Linq;

namespace ChaseServer
{
    class RoomMgr
    {
        public const int waitReadyTime = 30000;
        public const int gameTime = 60000;
        public const int maxMaps = 2;
        public const int maxPlayers = 5;
        public static RoomMgr instance;
        public static Random random = new Random();
        public RoomMgr()
        {
            instance = this;
        }

        public List<Room> list = new List<Room>();
        public List<Player> killers = new List<Player>();
        public List<Player> humans = new List<Player>();
        public Object queueLock = new object();

        public void CreateRoom(List<Player> players)
        {
            Room room = new Room();
            lock (list)
            {
                list.Add(room);
            }
            foreach (Player p in players)
            {
                p.tempData.isReady = false;
                room.AddPlayer(p);
            }

            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("EnterRoom");
            foreach (string id in room.list.Keys)
            {
                protocol.AddString(id);
            }
            room.Broadcast(protocol);
        }

        public void TestMatch()
        {
            lock (queueLock)
            {
                while (killers.Count >= 1 && humans.Count >= maxPlayers - 1)
                {
                    List<Player> players = new List<Player>();
                
                    players.Add(killers.First());
                    killers.RemoveAt(0);
                    for (int i = 0; i < 4; i++)
                    {
                        players.Add(humans.First());
                        humans.RemoveAt(0);
                    }
                    CreateRoom(players);
                }
            }
        }
    }
}
