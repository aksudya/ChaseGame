using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinPanel : PanelBase
{
    // Use this for initialization
    private Button CloseBtn;
    private Text EndText;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "WinPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowed()
    {
        base.OnShowed();
        Transform skinTrans = skin.transform;
        CloseBtn = skinTrans.FindChild("CloseBtn").GetComponent<Button>();
        EndText = skinTrans.FindChild("EndText").GetComponent<Text>();

        EndText.text = args[0].ToString();

        CloseBtn.onClick.AddListener(OnCloseClick);
    }

    public void OnCloseClick()
    {
        PanelMgr.instance.OpenPanel<StartPanl>("");
        //Walk.instance.StartGame(PanelMgr.instance.player_id);
        Close();
    }
}
