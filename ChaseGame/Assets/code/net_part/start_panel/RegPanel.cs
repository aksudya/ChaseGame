#define PUBLIC


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
public class RegPanel : PanelBase
{
    private InputField idInput;
    private InputField pwInput;
    private Button closeBtn;
    private Button regBtn;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RegPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowed()
    {
        base.OnShowed();
        Transform skinTrans = skin.transform;
        idInput = skinTrans.FindChild("IDInput").GetComponent<InputField>();
        pwInput = skinTrans.FindChild("PWInput").GetComponent<InputField>();
        closeBtn = skinTrans.FindChild("CloseBtn").GetComponent<Button>();
        regBtn = skinTrans.FindChild("RegBtn").GetComponent<Button>();

        closeBtn.onClick.AddListener(OnCloseClick);
        regBtn.onClick.AddListener(OnRegClick);
    }

    public void OnCloseClick()
    {
        PanelMgr.instance.OpenPanel<LoginPanel>("");
        Close();
    }

    public void OnRegClick()
    {
        if (idInput.text == "" || pwInput.text == "")
        {
            Debug.Log("用户名或密码不能为空！");
            return;
        }
        //建立连接
        if (NetMgr.srvConn.status != Connection.Status.connected)
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

        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Register");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        Debug.Log("发送:" + protocol.GetDesc());
        //发送login协议，并注册onloginback
        NetMgr.srvConn.Send(protocol, OnRegBack);
    }

    public void OnRegBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 200)
        {
            Debug.Log("注册成功！");
            PanelMgr.instance.OpenPanel<LoginPanel>("");
            Close();
        }
        else if(ret == 400)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：用户名不合法！");
            Debug.Log("用户名不合法！");
        }
        else if (ret == 401)
        {
            PanelMgr.instance.OpenPanel<WarningTips> ("","错误：密码不合法！");
            Debug.Log("密码不合法！");
        }
        else if (ret == 402)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：用户名已存在！");
            Debug.Log("用户名已存在！");
        }
        else if (ret == 500)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：数据库查询失败！");
            Debug.Log("数据库查询失败！");
        }
        else if (ret == 501)
        {
            PanelMgr.instance.OpenPanel<WarningTips>("", "错误：数据库写入失败！");
            Debug.Log("数据库写入失败！");
        }        
    }
}
