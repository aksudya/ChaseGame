
#define PUBLIC

using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : PanelBase {

    private InputField idInput;
    private InputField pwInput;
    private Button loginBtn;
    private Button regBtn;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "LoginPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowed()
    {
        base.OnShowed();
        Transform skinTrans = skin.transform;
        idInput = skinTrans.FindChild("IDInput").GetComponent<InputField>();
        pwInput= skinTrans.FindChild("PWInput").GetComponent<InputField>();
        loginBtn= skinTrans.FindChild("LoginBtn").GetComponent<Button>();
        regBtn = skinTrans.FindChild("RegBtn").GetComponent<Button>();

        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);
    }

    public void OnRegClick()
    {
        PanelMgr.instance.OpenPanel<RegPanel>("");
        Close();
    }

    public void OnLoginClick()
    {
        if (idInput.text==""||pwInput.text=="")
        {
            Debug.Log("用户名或密码不能为空！");
            return;
        }
        //建立连接
        if (NetMgr.srvConn.status!=Connection.Status.connected)
        {
#if (PUBLIC)
            string host = Dns.GetHostEntry("17ms539504.imwork.net").AddressList[0].ToString();
#else
            string host = "127.0.0.1";
#endif
            int port = 12345;
            NetMgr.srvConn.proto = new ProtocolBytes();
            NetMgr.srvConn.Connect(host, port);
        }
        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        Debug.Log("发送:" + protocol.GetDesc());
        //发送login协议，并注册onloginback
        NetMgr.srvConn.Send(protocol, OnLoginBack);

    }

    public void OnLoginBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret==200)
        {
            Debug.Log("登录成功！");
            PanelMgr.instance.player_id = idInput.text;
            PanelMgr.instance.OpenPanel<StartPanl>("");
            Close();
        }
        else if (ret == 400)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：用户名不合法！");
            Debug.Log("用户名不合法！");
        }
        else if (ret == 401)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：密码不合法！");
            Debug.Log("密码不合法！");
        }
        else if (ret == 403)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：用户名不存在！");
            Debug.Log("用户名不存在！");
        }
        else if (ret == 404)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：用户名或密码错误！");
            Debug.Log("用户名密码错误！");
        }
        else if (ret == 500)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：数据库查询失败！");
            Debug.Log("数据库查询失败！");
        }
        else if (ret == 501)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：kickoff数据库写入失败！");
            Debug.Log("kickoff数据库写入失败！");
        }
        else if (ret == 502)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：kickoff玩家数据序列化失败！");
            Debug.Log("kickoff玩家数据序列化失败！");
        }
    }
    // Use this for initialization
    
}
