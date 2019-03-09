using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningTips : PanelBase
{

    private Text WarningText;
    private Button OkBtn;
    private Button CancleBtn;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "WarningTips";
        layer = PanelLayer.Tips;
    }

    public override void OnShowed()
    {
        base.OnShowed();
        Transform skinTrans = skin.transform;
        WarningText = skinTrans.FindChild("Warning").GetComponent<Text>();
        OkBtn = skinTrans.FindChild("OkBtn").GetComponent<Button>();
        CancleBtn = skinTrans.FindChild("CancleBtn").GetComponent<Button>();

        WarningText.text = args[0].ToString();


        OkBtn.onClick.AddListener(OnOkClick);
        CancleBtn.onClick.AddListener(OnCancleClick);
    }

    public void OnOkClick()
    {
        Close();
    }

    public void OnCancleClick()
    {
        Close();
    }
}
