using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour {

    //摇杆
    public GameObject JoyStick;
    public ETCJoystick etcjoystick;
    //player身上的相机
    public GameObject camera;
    //玩家的图形素材
    public GameObject playerRotation;
    //玩家的id
    public GameObject playerIdText;
    //阵营
    public int camp;
    //操作类型
    public enum CtrlType
    {
        none,       //无
        human,      //玩家
        net,        //网络
    }
    

    public CtrlType ctrlType = CtrlType.human;
    //last 上次的位置信息
    Vector3 lPos;
    //forecast 预测的位置信息
    Vector3 fPos;
    float langle;
    //预测的旋转角度
    float fangle;
    //时间间隔
    float delta = 1;
    //上次接收的时间
    float lastRecvInfoTime = float.MinValue;
    //网络同步
    private float lastSendInfoTime = float.MinValue;
    // Use this for initialization
    //位置预测
    public void NetForecastInfo(Vector3 nPos)
    {
        //预测的位置
        fPos = lPos + (nPos - lPos) * 2;

        fangle = -Mathf.Atan2(nPos.x-lPos.x, nPos.y - lPos.y) * Mathf.Rad2Deg;
        //特殊网络情况，若延迟大于300ms
        if (Time.time - lastRecvInfoTime > 0.3f)
        {
            fPos = nPos;
            fangle = langle;
        }
        //时间间隔
        delta = Time.time - lastRecvInfoTime;
        //更新
        lPos = nPos;
        langle = fangle;
        lastRecvInfoTime = Time.time;
    }

    //初始化位置预测数据
    public void InitNetCtrl()
    {
        lPos = transform.position;
        fPos = transform.position;
        langle = playerRotation.transform.eulerAngles.z;
        fangle = playerRotation.transform.eulerAngles.z;
        Rigidbody2D r = GetComponent<Rigidbody2D>();
        r.constraints = RigidbodyConstraints2D.FreezeAll;
    }
    //更新位置
    public void NetUpdate()
    {
        //当前位置
        Vector3 pos = transform.position;
        Vector3 rot = playerRotation.transform.eulerAngles;
        //线性插值更新位置
        if(delta>0 && (Math.Abs(pos.x - lPos.x) > 0.01 || Math.Abs(pos.y - lPos.y) > 0.01))
        {

            transform.position = Vector3.Lerp(pos, fPos, delta);
            playerRotation.transform.rotation = Quaternion.Lerp(Quaternion.Euler(rot), Quaternion.Euler(0, 0, fangle), delta);
        }
    }

    //玩家控制
    public void JoyStickControlMove_move(Vector2 direction)
    {
        float moves_peed = 1.0f;
        if (camp==0)
        {
            moves_peed = 5.0f;
        }
        else
        {
            moves_peed = 4.0f;
        }
        if (ctrlType != CtrlType.human)
        {
            return;
        }

        float angle = -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        playerRotation.transform.rotation = Quaternion.Euler(0, 0, angle);
        Rigidbody2D _rigibody = GetComponent<Rigidbody2D>();
        _rigibody.MovePosition(new Vector3(transform.position.x + direction.x * moves_peed * Time.deltaTime, transform.position.y + direction.y * moves_peed * Time.deltaTime, 0));
    }

    //public void PlayerCtrl()
    //{
    //    if (ctrlType != CtrlType.human)
    //    {
    //        return;
    //    }


    //    if (Time.time - lastSendInfoTime > 0.2f)
    //    {
    //        SendUnitInfo();
    //        lastSendInfoTime = Time.time;
    //    }
    //}

    //发送位置信息
    public void SendUnitInfo()
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("UpdateInfo");
        //位置
        Vector3 pos = transform.position;
        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        NetMgr.srvConn.Send(proto);
    }


    void Start ()
    {
        JoyStick = GameObject.Find("New Joystick");
        etcjoystick=JoyStick.GetComponent<ETCJoystick>();
        if (ctrlType==CtrlType.human)
        {
            etcjoystick.onMove.AddListener(JoyStickControlMove_move);
        }

        
        //playerIdText.GetComponent<TextMesh>().text = gameObject.name;

    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{

    //    if(collision.gameObject.GetComponent<player>()!=null)
    //    {
    //        if (RoomMgr.instance.player_id[0] == PanelMgr.instance.player_id)
    //        {          
    //            if (camp == 0)
    //            {

    //                Destroy(collision.gameObject);
    //            }
    //        }
    //    }
    //}

    // Update is called once per frame
    void FixedUpdate()
    {
        if (ctrlType == CtrlType.net)
        {
            NetUpdate();
            return;
        }
        if (ctrlType == CtrlType.human)
        {
            if (Time.time - lastSendInfoTime > 0.06f)
            {
                SendUnitInfo();
                lastSendInfoTime = Time.time;
            }
        }
    }
}
