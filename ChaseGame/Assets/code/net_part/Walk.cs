using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Walk : MonoBehaviour
{
    public GameObject prefab;
    //players
    Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    //self
    string playerID = "";
    //上一次移动的时间
    public float times;
    //单例
    public static Walk instance;
    private void Start()
    {
        instance = this;
    }

    void AddPlayer(string id, Vector3 pos, int score)
    {
        GameObject player = (GameObject)Instantiate(prefab, pos, Quaternion.identity);
        players.Add(id, player);
        TextMesh textmesh = player.GetComponentInChildren<TextMesh>();
        textmesh.text = id;
        //将非自己的camera删除
        if (id != playerID)
        {            
            foreach (Transform child in player.GetComponentsInChildren<Transform>())
            {
                if (child.name == "Camera")
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    void DelPlayer(string id)
    {
        if (players.ContainsKey(id))
        {
            Destroy(players[id]);
            players.Remove(id);
        }
    }

    public void UpdateInfo(string id, Vector3 pos, int score)
    {
        //玩家自己
        if (id == playerID)
        {
            //players[id].transform.position = pos;
        }
        lock (players)
        {
            if (players.ContainsKey(id))
            {
                players[id].transform.position = pos;
            }
            else
            {
                AddPlayer(id, pos, score);
            }
        }
    }

    public void StartGame(string id)
    {
        playerID = id;
        //产生自己
        Vector3 pos = new Vector3(0, 0, -1);
        AddPlayer(playerID, pos, 0);
        //同步
        SendPos();
        //获取列表
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("GetList");
        NetMgr.srvConn.Send(proto, GetList);
        NetMgr.srvConn.msgDist.AddListener("UpdateInfo", UpdateInfo);
        NetMgr.srvConn.msgDist.AddListener("PlayerLeave", PlayerLeave);

    }

    

    void SendPos()
    {
        GameObject player = players[playerID];
        Vector3 pos = player.transform.position;
        //消息
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("UpdateInfo");
        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        NetMgr.srvConn.Send(proto);
    }

    public void GetList(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        //获取头部数据
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);
        for (int i=0;i<count;i++)
        {
            string id = proto.GetString(start, ref start);
            float x = proto.GetFloat(start, ref start);
            float y = proto.GetFloat(start, ref start);
            int score= proto.GetInt(start, ref start);
            Vector3 pos = new Vector3(x, y, -1);
            UpdateInfo(id, pos, score);
        }
    }
    //更新数据
    public void UpdateInfo(ProtocolBase protocol)
    {
        //获取数据
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        float x = proto.GetFloat(start, ref start);
        float y = proto.GetFloat(start, ref start);
        int score = proto.GetInt(start, ref start);
        Vector3 pos = new Vector3(x, y, -1);
        UpdateInfo(id, pos, score);
    }

    public void PlayerLeave(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        DelPlayer(id);
    }

    private float angle;
    private Rigidbody2D _rigibody;
    public float moves_peed = 5.0f;
    int canmove = 1;
    public void JoyStickControlMove_move(Vector2 direction)
    {
        if(playerID=="")
        {
            return;
        }
        if (players[playerID]==null)
        {
            return;
        }
        GameObject player = players[playerID];
        _rigibody = player.GetComponent<Rigidbody2D>();
        _rigibody.MovePosition(new Vector3(player.transform.position.x + direction.x * moves_peed * Time.deltaTime, player.transform.position.y + direction.y * moves_peed * Time.deltaTime, 0));
    }

    private void Update()
    {
        if(times<=0.02)
        {
            times += Time.deltaTime;
        }
        else
        {
            canmove = 1;
        }
        if (canmove != 0)
        {
            SendPos();
            times = 0;
            canmove = 0;
            
        }
    }

}

