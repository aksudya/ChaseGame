using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomMgr : MonoBehaviour
{
    //匹配是否完成
    public int ismatching = 1;
    //实例
    public static RoomMgr instance;
    //获取所有玩家的id
    public List<string> player_id = new List<string>();
    private void Start()
    {
        instance = this;
    }

    public void startMatching()
    {
        if(NetMgr.srvConn.msgDist.ContainOnceListener("EnterRoom"))
        {
            NetMgr.srvConn.msgDist.DelOnceListener("EnterRoom", EnterRoomBack);
        }
        NetMgr.srvConn.msgDist.AddOnceListener("EnterRoom", EnterRoomBack);
    }

    //所有玩家进入准备阶段
    public void EnterRoomBack(ProtocolBase protocol)
    {
        lock(player_id)
        {
            if(player_id.Count!=0)
            {
                player_id.Clear();
            }
        }
        Debug.Log("匹配完成！");
        ismatching = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        lock(player_id)
        {
            for (int i = 0; i < BattleMgr_net.instance.player_count; i++)
            {
                string id = proto.GetString(start, ref start);
                player_id.Add(id);
            }
        }
        PanelMgr.instance.OpenPanel<RoomPanel>("");
        PanelMgr.instance.ClosePanel("MatchingPanel");
        //NetMgr.srvConn.msgDist.AddListener("ReadyFight", ReadyFightBack);
        if (NetMgr.srvConn.msgDist.ContainOnceListener("StartLoad"))
        {
            NetMgr.srvConn.msgDist.DelOnceListener("StartLoad", StartLoadBack);
        }
        NetMgr.srvConn.msgDist.AddOnceListener("StartLoad", StartLoadBack);
        //NetMgr.srvConn.msgDist.AddOnceListener("LeaveRoom", LeaveBack);
        //NetMgr.srvConn.msgDist.AddOnceListener("RoomDestroy", DestroyBack);

        //NetMgr.srvConn.msgDist.AddListener("Nomatch", NomatchBack);

    }

    //玩家准备
    //public void ReadyFightBack(ProtocolBase protocol)
    //{
    //    ProtocolBytes proto = (ProtocolBytes)protocol;
    //    int start = 0;
    //    string protoName = proto.GetString(start, ref start);

    //}


    //所有玩家已准备，开始加载
    public void StartLoadBack(ProtocolBase protocol)
    {
        
        PanelMgr.instance.OpenPanel<LoadingPanel>("");
        PanelMgr.instance.ClosePanel("RoomPanel");
        ProtocolBytes proto = (ProtocolBytes)protocol;
        BattleMgr_net.instance.StartBattle(proto);

    }

    ////通知玩家房间已销毁，并重新匹配
    //public void RematchBack(ProtocolBase protocol)
    //{      
    //    PanelMgr.instance.OpenPanel<MatchingPanel>("");
    //    PanelMgr.instance.ClosePanel("RoomPanel");
    //}

    //通知玩家房间已销毁
    public void ReMatch()
    {

        if(player_id[0]==PanelMgr.instance.player_id)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("EnterKiller");
            Debug.Log("发送:" + protocol.GetDesc());
            NetMgr.srvConn.Send(protocol, OnKillerBack);
        }
        else
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("EnterHuman");
            Debug.Log("发送:" + protocol.GetDesc());
            NetMgr.srvConn.Send(protocol, OnHumanBack);
        }


        //startMatching();
        //PanelMgr.instance.OpenPanel<MatchingPanel>("");
        //PanelMgr.instance.ClosePanel("RoomPanel");
    }

    public void OnKillerBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 200)
        {
            Debug.Log("成功加入屠夫匹配队列！");

            PanelMgr.instance.OpenPanel<MatchingPanel>("");
            PanelMgr.instance.ClosePanel("RoomPanel");
            startMatching();
            //Close();
        }
        else if (ret == 417)
        {
            //PanelMgr.instance.OpenPanel<WarningTips>("", "错误：加入匹配队列失败，请重试");
            Debug.Log("加入匹配队列失败，请重试");
            ReMatch();
        }
    }

    public void OnHumanBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 200)
        {
            Debug.Log("成功加入人类匹配队列！");

            PanelMgr.instance.OpenPanel<MatchingPanel>("");
            PanelMgr.instance.ClosePanel("RoomPanel");
            startMatching();
            //Close();
        }
        else if (ret == 417)
        {
            //PanelMgr.instance.OpenPanel<WarningTips>("", "错误：加入匹配队列失败，请重试");
            Debug.Log("加入匹配队列失败，请重试");
            ReMatch();
        }
    }


    public void QuitToStart()
    {

        PanelMgr.instance.OpenPanel<StartPanl>("");
        PanelMgr.instance.ClosePanel("RoomPanel");
    }

}
