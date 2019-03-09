namespace ChaseServer
{
    class Player
    {
        public string id;
        public Conn conn;
        public PlayerData data;
        public PlayerTempData tempData;
        public SceneData sceneData;

        public Player(string id, Conn conn)
        {
            this.id = id;
            this.conn = conn;
            tempData = new PlayerTempData();
        }

        public void Send(ProtocolBase proto)
        {
            if (conn == null)
                return;
            ServNet.instance.Send(conn, proto);
        }

        public static int KickOff(string id, ProtocolBase proto)
        {
            Conn[] conns = ServNet.instance.conns;
            for (int i = 0; i < conns.Length; i++)
            {
                Conn conn = conns[i];
                if (conn == null || !conn.isUse) continue;
                if (conn.player == null)
                    continue;
                if (conn.player.id == id)
                {
                    lock (conn.player)
                    {
                        if (proto != null)
                            conn.player.Send(proto);
                        return conn.player.Logout();
                    }
                }
            }
            return 200;
        }

        public int Logout()
        {
            ServNet.instance.handlePlayerEvent.OnLogout(this);
            int spstate = DataMgr.instance.SavePlayer(this);
            if (spstate != 200)
                return spstate;
            conn.player = null;
            conn.Close();
            conn = null;
            return 200;
        }
    }
}
