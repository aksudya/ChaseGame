using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreTips : PanelBase
{
    private Text HunmanText;
    private Text KillerText;
    private Button ReturnBtn;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "ScoreTips";
        layer = PanelLayer.Tips;
    }

    public override void OnShowed()
    {
        base.OnShowed();
        Transform skinTrans = skin.transform;
        HunmanText = skinTrans.FindChild("HunmanText").GetComponent<Text>();
        KillerText = skinTrans.FindChild("KillerText").GetComponent<Text>();
        ReturnBtn = skinTrans.FindChild("ReturnBtn").GetComponent<Button>();


        HunmanText.text = "幸存者分数："+args[0].ToString();
        KillerText.text = "屠夫分数：" + args[1].ToString();


        ReturnBtn.onClick.AddListener(OnReturnClick);
    }

    public void OnReturnClick()
    {
        Close();
    }
}

