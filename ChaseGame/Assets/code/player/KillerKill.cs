using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillerKill : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.GetComponent<player>() != null)
        {
            if (RoomMgr.instance.player_id[0] == PanelMgr.instance.player_id)
            {
                ProtocolBytes proto = new ProtocolBytes();
                proto.AddString("KilledHuman");
                proto.AddString(collision.gameObject.name);
                NetMgr.srvConn.Send(proto);
            }
        }
    }

    
}
