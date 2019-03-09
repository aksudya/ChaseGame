using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingPanel : PanelBase
{
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "LoadingPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowed()
    {
        base.OnShowed();
    }
}
