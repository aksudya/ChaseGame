using System.Collections.Generic;

namespace ChaseServer
{
    class Scene
    {
        public static Scene instance;
        public Scene()
        {
            instance = this;
        }

        Dictionary<string, ScenePlayer> list = new Dictionary<string, ScenePlayer>();

        public void AddPlayer(string id)
        {
            lock (list)
            {
                ScenePlayer p = new ScenePlayer();
                p.id = id;
                list.Add(id, p);
            }
        }

        public void DelPlayer(string id)
        {
            lock (list)
            {
                if (!list.Remove(id))
                    return;
            }
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("PlayerLeave");
            protocol.AddString("id");
            ServNet.instance.Broadcast(protocol);
        }

        public void SendPlayerList(Player player)
        {
            int count = list.Count;
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("GetList");
            protocol.AddInt(count);
            foreach (ScenePlayer p in list.Values)
            {
                protocol.AddString(p.id);
                protocol.AddFloat(p.x);
                protocol.AddFloat(p.y);
                protocol.AddInt(p.score);
            }
            player.Send(protocol);
        }

        public void UpdateInfo(string id, float x, float y, int score)
        {
            ScenePlayer p = list[id];
            if (p == null) return;

            p.x = x;
            p.y = y;
            p.score = score;
        }
    }
}
