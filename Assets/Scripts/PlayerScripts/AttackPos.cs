using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPos : MonoBehaviour
{
    int direction;

    // Start is called before the first frame update

    void Start()
    {
        Cainos.PixelArtTopDown_Basic.TopDownCharacterController.direction = 1;
    }

    // Update is called once per frame
    void Update()
    {
        direction = Cainos.PixelArtTopDown_Basic.TopDownCharacterController.direction;
        if(direction == 0)
        {
            gameObject.transform.localPosition = new Vector2(0, -1);
        }else if (direction == 1)
        {
            gameObject.transform.localPosition = new Vector2(0, 1);
        }
        else if (direction == 2)
        {
            gameObject.transform.localPosition = new Vector2(1, 0);
        }
        else if (direction == 3)
        {
            gameObject.transform.localPosition = new Vector2(-1, 0);
        }
    }
}
