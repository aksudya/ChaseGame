using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cha_aim : MonoBehaviour {
    private float angle;
    public void JoyStickControlMove_aim(Vector2 direction)
    {
        angle = -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        this.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
