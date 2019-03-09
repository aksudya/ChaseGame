using System;

namespace ChaseServer
{
    class MainClass
    {
        public static int listenPort = 12345;
        public static ServNet servNet;
        public static DataMgr dataMgr;
        public static RoomMgr roomMgr;

        public static void Main(string[] args)
        {
            dataMgr = new DataMgr();
            servNet = new ServNet();
            roomMgr = new RoomMgr();
            servNet.proto = new ProtocolBytes();
            //servNet.Start("192.168.1.107", listenPort);
            servNet.Start("127.0.0.1", listenPort);
            
            while (true)
            {
                string str = Console.ReadLine();
                switch (str)
                {
                    case "quit":
                        servNet.Close();
                        return;
                    case "print":
                        ServNet.instance.Print();
                        break;
                    default:
                        Console.WriteLine("[非法输入]");
                        break;
                }
            }
        }
    }
}
