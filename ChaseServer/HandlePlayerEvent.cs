namespace ChaseServer
{
    class HandlePlayerEvent
    {
        public void OnLogin(Player player)
        {
        }

        public void OnLogout(Player player)
        {
            if (player.tempData.state == PlayerTempData.State.Queue)
            {
                lock (RoomMgr.instance.queueLock)
                {
                    if (player.tempData.isKiller)
                        RoomMgr.instance.killers.Remove(player);
                    else
                        RoomMgr.instance.humans.Remove(player);
                }
            }

            Room room = player.tempData.room;
            if (room != null)
            {
                if (room.state == Room.State.Load && !player.tempData.isOnload)
                {
                    player.tempData.isOnload = true;
                    room.onloadPlayers++;
                }
                else if (room.state == Room.State.Fight)
                {
                    if (player.tempData.isKiller)
                    {
                        room.GameOver(2);
                    }
                    else
                        room.KilledHuman(player);
                }
            }
        }
    }
}
