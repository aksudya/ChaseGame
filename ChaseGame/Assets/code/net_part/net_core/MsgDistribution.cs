using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//消息分发
public class MsgDistribution 
{
    //每帧处理的消息数
    public int num = 15;
    //消息列表 
    public List<ProtocolBase> msgList = new List<ProtocolBase>();
    //委托类型
    public delegate void Delegate(ProtocolBase proto);
    //事件监听表
    private Dictionary<string, Delegate> eventDirt = new Dictionary<string, Delegate>();
    private Dictionary<string, Delegate> onceDirt = new Dictionary<string, Delegate>();

    public void Update()
    {
        for (int i = 0; i < num; i++)
        {
            if(msgList.Count>0)
            {
                DispatchMsgEvent(msgList[0]);
                lock (msgList)
                    msgList.RemoveAt(0);    //避免线程竞争
            }
            else
            {
                break;
            }
        }
    }

    public void DispatchMsgEvent(ProtocolBase protocol)
    {
        string name = protocol.GetName();
        Debug.Log("分发处理消息" + name);
        if (eventDirt.ContainsKey(name))
        {
            eventDirt[name](protocol);
        }
        if (onceDirt.ContainsKey(name))
        {
            onceDirt[name](protocol);
            onceDirt[name] = null;
            onceDirt.Remove(name);
        }
    }

    public void AddListener(string name,Delegate cb)
    {
        if (eventDirt.ContainsKey(name))
        {
            eventDirt[name] += cb;
        }
        else
            eventDirt[name] = cb;
    }

    public void AddOnceListener(string name,Delegate cb)
    {
        if(onceDirt.ContainsKey(name))
        {
            onceDirt[name] += cb;
        }
        else
        {
            onceDirt[name] = cb;
        }
    }

    public void DelListener(string name,Delegate cb)
    {
        if(eventDirt.ContainsKey(name))
        {
            eventDirt[name] -= cb;
            if(eventDirt[name]==null)
            {
                eventDirt.Remove(name);
            }
        }
    }

    public void DelOnceListener(string name, Delegate cb)
    {
        if (onceDirt.ContainsKey(name))
        {
            onceDirt[name] -= cb;
            if (onceDirt[name] == null)
            {
                onceDirt.Remove(name);
            }
        }
    }

    public bool ContainOnceListener(string name)
    {
        if (onceDirt.ContainsKey(name))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
