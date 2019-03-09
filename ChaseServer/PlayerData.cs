using System;

namespace ChaseServer
{
    [Serializable]
    class PlayerData
    {
        private const int scoreFloor = 0;
        private const int scoreUpper = 100;
        private int scoreHuman;
        private int scoreKiller;
        public int ScoreHuman
        {
            get
            {
                return scoreHuman;
            }
            set
            {
                if (value >= scoreFloor && value <= scoreUpper)
                {
                    scoreHuman = value;
                }
            }
        }
        public int ScoreKiller
        {
            get
            {
                return scoreKiller;
            }
            set
            {
                if (value >= scoreFloor && value <= scoreUpper)
                {
                    scoreKiller = value;
                }
            }
        }

        public PlayerData()
        {
            scoreHuman = 0;
            scoreKiller = 0;
        }
    }
}
