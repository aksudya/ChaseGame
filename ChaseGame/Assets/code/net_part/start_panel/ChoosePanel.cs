using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePanel : PanelBase
{
    private Button killerBtn;
    private Button humanBtn;
    private Button closeBtn;
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "ChoosePanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowed()
    {
        base.OnShowed();
        Transform skinTrans = skin.transform;
        killerBtn = skinTrans.FindChild("killerBtn").GetComponent<Button>();
        humanBtn = skinTrans.FindChild("humanBtn").GetComponent<Button>();
        closeBtn = skinTrans.FindChild("closeBtn").GetComponent<Button>();
        killerBtn.onClick.AddListener(OnkillerClick);
        humanBtn.onClick.AddListener(OnhumanClick);
        closeBtn.onClick.AddListener(OnCloseClick);
    }

    //选择屠夫
    public void OnkillerClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("EnterKiller");
        Debug.Log("发送:" + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol, OnKillerBack);
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
            RoomMgr.instance.startMatching();
            Close();
        }
        else if (ret == 417)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：加入匹配队列失败，请重试");
            Debug.Log("加入匹配队列失败，请重试");
        }
        //NetMgr.srvConn.msgDist.AddListener("EnterRoom",CreatRoom);

    }
    //选择人类
    public void OnhumanClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("EnterHuman");
        Debug.Log("发送:" + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol, OnHumanBack);
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
            RoomMgr.instance.startMatching();
            Close();
        }
        else if(ret == 417)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：加入匹配队列失败，请重试");
            Debug.Log("加入匹配队列失败，请重试");
        }
    }
    public void OnCloseClick()
    {
        PanelMgr.instance.OpenPanel<StartPanl>("");
        Close();
    }
}
