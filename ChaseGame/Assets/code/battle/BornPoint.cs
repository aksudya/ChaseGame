using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BornPoint : MonoBehaviour {

    

    public static BornPoint instance;
    public Vector3[,] bornpoints=new Vector3[2,5];
    
    private void Start()
    {
        
        instance = this;

        bornpoints[0, 0] = new Vector3(0.0f,0.0f,-1.0f);
        bornpoints[0, 1] = new Vector3(7.0f, 5.0f, -1.0f);
        bornpoints[0, 2] = new Vector3(7.0f, -5.0f, -1.0f);
        bornpoints[0, 3] = new Vector3(-7.0f, -5.0f, -1.0f);
        bornpoints[0, 4] = new Vector3(-7.0f, 5.0f, -1.0f);

        bornpoints[1, 0] = new Vector3(0.0f, 8.0f, -1.0f);
        bornpoints[1, 1] = new Vector3(8.0f, -8.0f, -1.0f);
        bornpoints[1, 2] = new Vector3(2.0f, -8.0f, -1.0f);
        bornpoints[1, 3] = new Vector3(-2.0f, -8.0f, -1.0f);
        bornpoints[1, 4] = new Vector3(-8.0f, -8.0f, -1.0f);

        
    }


    
}
