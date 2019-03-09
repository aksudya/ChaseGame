using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class BattleMgr_net : MonoBehaviour
{
    public int player_count = 5;    //一局的玩家人数
    //玩家预设
    public GameObject prefabKiller;
    public GameObject prefabHuman;
    //players
    Dictionary<string, player> players = new Dictionary<string, player>();
    //游戏内倒计时
    public GameObject TimerText;
    private float timer = 60;
    private bool isStart = false;
    //单例
    public static BattleMgr_net instance;
    private void Start()
    {
        instance = this;
    }

    public void StartBattle(ProtocolBytes proto)
    {
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        if (protoName != "StartLoad")
        {
            return;
        }
        int swopID_group = proto.GetInt(start, ref start);
        /***初始化战场****/
        lock (players)
        {
            if (players.Count != 0)
            {
                players.Clear();
            }
            for (int i = 0; i < player_count; i++)
            {
                string id = RoomMgr.instance.player_id[i];
                int team;
                if (i == 0)
                {
                    team = 0;   //屠夫
                }
                else
                {
                    team = 1;
                }
                //int swopID_group = proto.GetInt(start, ref start);
                int swopID_place = i;
                GeneratePlayer(id, team, swopID_group, swopID_place);
            }
        }


        /***************/

        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("MapOnload");
        Debug.Log("发送:" + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol);
        if (NetMgr.srvConn.msgDist.ContainOnceListener("StartFight"))
        {
            NetMgr.srvConn.msgDist.DelOnceListener("StartFight", RecvStartFight);
        }
        NetMgr.srvConn.msgDist.AddOnceListener("StartFight", RecvStartFight);

        //PanelMgr.instance.ClosePanel("LoadingPanel");
        //NetMgr.srvConn.msgDist.AddListener("UpdateInfo", RecvUpdateInfo);
    }

    public void RecvStartFight(ProtocolBase protocol)
    {
        PanelMgr.instance.ClosePanel("LoadingPanel");
        NetMgr.srvConn.msgDist.AddListener("KilledHuman", RecvKilledHuman);
        NetMgr.srvConn.msgDist.AddListener("UpdateInfo", RecvUpdateInfo);
        NetMgr.srvConn.msgDist.AddOnceListener("GameOver", RecvGameOver);
        isStart = true;
        TimerText.GetComponent<Text>().text= ((int)timer).ToString();

    }

    public void RecvGameOver(ProtocolBase protocol)
    {
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        //TimerText.GetComponent<Text>().text = "";
        //timer = 60;
        //isStart = false;
        if (ret==1)  //屠夫胜利
        {
            if(PanelMgr.instance.player_id==RoomMgr.instance.player_id[0])//本机是屠夫
            {
                PanelMgr.instance.OpenPanel<WinPanel>("", "恭喜，你杀掉了所有幸存者");
            }
            else    //本机是幸存者
            {
                PanelMgr.instance.OpenPanel<WinPanel>("", "任务失败，所有幸存者都被杀掉");
            }
        }
        else if(ret==2)//幸存者胜利
        {
            if (PanelMgr.instance.player_id == RoomMgr.instance.player_id[0])//本机是屠夫
            {
                PanelMgr.instance.OpenPanel<WinPanel>("", "时间到，很不幸，还有幸存者活了下来");
            }
            else    //本机是幸存者
            {
                PanelMgr.instance.OpenPanel<WinPanel>("", "时间到，还有幸存者活了下来，任务成功");
            }
        }
        
        resetBattle();
    }


    public void RecvKilledHuman(ProtocolBase protocol)
    {
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        string humanID = proto.GetString(start, ref start);
        player pl = players[humanID];
        Destroy(pl.gameObject);
        players.Remove(humanID);
        //if(humanID==PanelMgr.instance.player_id)
        //{
        //}
    }


    public void GeneratePlayer(string id,int team,int swswopID_group,int swopID_place)
    {
        if (team==0)
        {
            GameObject Killer = (GameObject)Instantiate(prefabKiller);
            Killer.transform.position = BornPoint.instance.bornpoints[swswopID_group, swopID_place];
            Killer.name = id;
            //Killer.GetComponent<TextMesh>().text = id;
            player kl = new player();
            kl=Killer.GetComponent<player>();
            kl.playerIdText.GetComponent<TextMesh>().text = id;
            kl.camp = 0;
            players.Add(id, kl);
            if (id == PanelMgr.instance.player_id)
            {
                kl.ctrlType = player.CtrlType.human;
                
            }
            else
            {
                kl.camera.SetActive(false);
                kl.ctrlType = player.CtrlType.net;
                kl.InitNetCtrl();  //初始化网络同步
            }
        }
        else
        {
            GameObject human = (GameObject)Instantiate(prefabHuman);
            human.transform.position = BornPoint.instance.bornpoints[swswopID_group, swopID_place];
            human.name = id;
            //human.GetComponent<TextMesh>().text = id;
            player hm = new player();
            hm = human.GetComponent<player>();
            hm.playerIdText.GetComponent<TextMesh>().text = id;
            hm.camp = 1;
            
            players.Add(id, hm);
            if (id == PanelMgr.instance.player_id)
            {
                hm.ctrlType = player.CtrlType.human;
            }
            else
            {
                hm.camera.SetActive(false);
                hm.ctrlType = player.CtrlType.net;
                hm.InitNetCtrl();  //初始化网络同步
            }
        }

        
    }

    //更新信息
    public void RecvUpdateInfo(ProtocolBase protocol)
    {
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        Vector3 nPos;
        nPos.x = proto.GetFloat(start, ref start);
        nPos.y = proto.GetFloat(start, ref start);
        nPos.z = -1;
        Debug.Log("RecvUpdateInfo " + id);
        if (!players.ContainsKey(id))
        {
            Debug.Log("RecvUpdateInfo bt == null ");
            return;
        }
        player pl = players[id];
        if (id == PanelMgr.instance.player_id)
            return;

        pl.NetForecastInfo(nPos);
    }


    public void resetBattle()
    {
        NetMgr.srvConn.msgDist.DelListener("KilledHuman", RecvKilledHuman);
        NetMgr.srvConn.msgDist.DelListener("UpdateInfo", RecvUpdateInfo);
        NetMgr.srvConn.msgDist.DelOnceListener("GameOver", RecvGameOver);
        lock (players)
        {
            foreach (player Values in players.Values)
            {

                Destroy(Values.gameObject);
            }
        }
        isStart = false;
        TimerText.GetComponent<Text>().text = "";
        timer = 60;
        
    }

    public void Update()
    {
        if(isStart)
        {
            timer -= Time.deltaTime;
            TimerText.GetComponent<Text>().text = ((int)timer).ToString();
        }
    }
}



