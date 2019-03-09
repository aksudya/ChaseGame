using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchingPanel : PanelBase
{

    private Button LeaveBtn;
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "MatchingPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowed()
    {
        base.OnShowed();
        Transform skinTrans = skin.transform;
        LeaveBtn = skinTrans.FindChild("LeaveBtn").GetComponent<Button>();
        LeaveBtn.onClick.AddListener(OnLeaveBtnClick);
    }

    //按下离开匹配按钮
    public void OnLeaveBtnClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("LeaveQueue");
        Debug.Log("发送:" + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol, OnLeaveBack);
    }
    //处理回调
    public void OnLeaveBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 200)
        {
            Debug.Log("退出匹配成功！");
            PanelMgr.instance.OpenPanel<ChoosePanel>("");
            Close();
        }
        else if (ret == 417)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：玩家状态错误！");
            Debug.Log("玩家状态错误！");
        }
        else if (ret == 500)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：玩家未在数组内！");
            Debug.Log("玩家未在数组内！");
        }
    }

}

