using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPanl : PanelBase
{

    // Use this for initialization
    private Button startBtn;
    private Button ScoreBtn;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "StartPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowed()
    {
        base.OnShowed();
        Transform skinTrans = skin.transform;    
        startBtn = skinTrans.FindChild("StartBtn").GetComponent<Button>();
        ScoreBtn = skinTrans.FindChild("ScoreBtn").GetComponent<Button>();

        ScoreBtn.onClick.AddListener(OnScoreClick);
        startBtn.onClick.AddListener(OnStartClick);       
    }

    public void OnStartClick()
    {
        PanelMgr.instance.OpenPanel<ChoosePanel>("");
        //Walk.instance.StartGame(PanelMgr.instance.player_id);
        Close();
    }
    public void OnScoreClick()
    {

        //PanelMgr.instance.OpenPanel<ScoreTips>("");
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetScore");
        Debug.Log("发送:" + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol, OnScoreBack);
    }

    public void OnScoreBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ScoreHuman = proto.GetInt(start, ref start);
        int ScoreKiller = proto.GetInt(start, ref start);

        PanelMgr.instance.OpenPanel<ScoreTips>("",ScoreHuman,ScoreKiller);
    }
}
