using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cha_move : MonoBehaviour {
    private float angle;
    private Rigidbody2D _rigibody;
    public float moves_peed = 2.0f;   
    public void JoyStickControlMove_move(Vector2 direction)
    {
        _rigibody = this.GetComponent<Rigidbody2D>();
        _rigibody.MovePosition(new Vector3(this.transform.position.x + direction.x * moves_peed * Time.deltaTime, this.transform.position.y + direction.y * moves_peed * Time.deltaTime, 0));
    }
    
    
}
