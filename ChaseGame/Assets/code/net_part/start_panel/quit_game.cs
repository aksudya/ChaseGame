using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class quit_game : MonoBehaviour {
    public GameObject quit_button;
    //public GameObject star_panel;

	// Use this for initialization
	void Start ()
    {
        quit_button.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void OnQuitClick()
    {

        //if(NetMgr.srvConn.status == Connection.Status.connected)
        //{
        //    NetMgr.srvConn.Close();
        //}

        quit_button.SetActive(false);
        PanelMgr.instance.OpenPanel<StartPanl>("");
        //star_panel.SetActive(true);
    }
}
