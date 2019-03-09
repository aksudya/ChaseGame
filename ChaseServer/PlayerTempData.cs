namespace ChaseServer
{
    class PlayerTempData
    {
        public enum State
        {
            None, Queue, Room, Fight,
        }
        public State state = State.None;

        public Room room;
        public bool isReady;
        public bool isOnload;
        public bool isKiller = false;
    }
}
