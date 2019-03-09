using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RoomPanel : PanelBase
{
    //倒计时
    private Text CountTimer;
    private float timer=30;
    private bool isloaded=false;
    //private Text PlayerIds;
    private Text[] PlayerIds = new Text[5];
    //private GameObject isReady;
    private Button ReadyBtn;
    private Button LeaveBtn;
    //已准备
    private bool isReady=false;
    //图片
    //public GameObject isReadyPic;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RoomPanel";
        layer = PanelLayer.Panel;
        NetMgr.srvConn.msgDist.AddListener("ReadyFight", ReadyFightBack);
        if (NetMgr.srvConn.msgDist.ContainOnceListener("LeaveRoom"))
        {
            NetMgr.srvConn.msgDist.DelOnceListener("LeaveRoom", LeaveRoomBack);
        }
        NetMgr.srvConn.msgDist.AddOnceListener("LeaveRoom", LeaveRoomBack);
    }
    public override void OnShowed()
    {
        base.OnShowed();
        Transform skinTrans = skin.transform;
        CountTimer = skinTrans.FindChild("TimeText").GetComponent<Text>();

        //isReady = skinTrans.FindChild("IsReady").GetComponent<GameObject>();
        //isReady.SetActive(false);

        //PlayerIds = skinTrans.FindChild("KillerText").GetComponent<Text>();

        PlayerIds[0] = skinTrans.FindChild("KillerText").GetComponent<Text>();
        PlayerIds[1] = skinTrans.FindChild("HumanText1").GetComponent<Text>();
        PlayerIds[2] = skinTrans.FindChild("HumanText2").GetComponent<Text>();
        PlayerIds[3] = skinTrans.FindChild("HumanText3").GetComponent<Text>();
        PlayerIds[4] = skinTrans.FindChild("HumanText4").GetComponent<Text>();

        ReadyBtn = skinTrans.FindChild("ReadyBtn").GetComponent<Button>();
        LeaveBtn = skinTrans.FindChild("LeaveBtn").GetComponent<Button>();

        ReadyBtn.onClick.AddListener(OnReadyClick);
        LeaveBtn.onClick.AddListener(OnLeaveClick);
        //isReadyPic.SetActive(false);


        lock (RoomMgr.instance.player_id)
        {
            int count=0;
            foreach (string ids in RoomMgr.instance.player_id)
            {
                //PlayerIds.text = ids;
                PlayerIds[count].text = ids;
                count++;
            }
        }       
        
        CountTimer.text = ((int)timer).ToString();
        isloaded = true;
    }

    //按下准备按钮
    public void OnReadyClick()
    {
        if (!isReady)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("ReadyFight");
            Debug.Log("发送:" + protocol.GetDesc());
            NetMgr.srvConn.Send(protocol, OnReadyBack);
        }

    }

    public void ReadyFightBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        if (id == PanelMgr.instance.player_id)
        {
            //int place = RoomMgr.instance.player_id.FindIndex(item => item.Equals(id));
            //PlayerIds[place].color = Color.green;
            return;
        }
        else
        {
            Debug.Log(id+":准备成功！");
            int place = RoomMgr.instance.player_id.FindIndex(item => item.Equals(id));
            PlayerIds[place].color = Color.green;
        }
    }


    //处理回调
    public void OnReadyBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        if (id != PanelMgr.instance.player_id)
        {            
            return;
        }
        int ret = proto.GetInt(start, ref start);
        if (ret == 200)
        {
            Debug.Log("准备成功！");
            isReady = true;
            //isReadyPic.SetActive(true);
            int place = RoomMgr.instance.player_id.FindIndex(item => item.Equals(id));
            PlayerIds[place].color = Color.green;
            //isReady.SetActive(true);
            
        }
        else if(ret== 417)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("错误：玩家状态错误！");
            Debug.Log("玩家状态错误！");
        }
        else if(ret == 418)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("错误：房间已删除！");
            Debug.Log("房间已删除！");
        }
    }


    //按下离开按钮
    public void OnLeaveClick()
    {

        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("LeaveRoom");
        Debug.Log("发送:" + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol);
    }
    //处理回调
    public void LeaveRoomBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 408)
        {
            Debug.Log("房间超时！");
            RoomMgr.instance.QuitToStart();
            //PanelMgr.instance.OpenPanel<StartPanl>("");
            //Close();
        }
        else if (ret == 410)
        {
            Debug.Log("玩家退出！");
            string id= proto.GetString(start, ref start);
            if (id == PanelMgr.instance.player_id)        //按离开的那个人
            {
                RoomMgr.instance.QuitToStart();
            }
            else                                        //其他人
            {
                RoomMgr.instance.ReMatch();
            }
        }
        
    }




    public override void OnClosing() 
    {
        NetMgr.srvConn.msgDist.DelListener("ReadyFight", ReadyFightBack);
        base.OnClosing();
    }


    public override void Update()
    {
        if (isloaded)
        {
            timer -= Time.deltaTime;
            CountTimer.text = ((int)timer).ToString();
        }

    }
}
