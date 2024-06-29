using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyStick : MonoBehaviour
{
    public Transform stick;
    public Canvas canvas;
    public float max_R = 80;
    // Start is called before the first frame update
    void Start()
    {
        stick.localPosition = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void on_begin_drag()
    {

    }

    public void on_end_drag()
    {
        stick.localPosition = Vector2.zero;
    }

    public void on_drag()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, Input.mousePosition, canvas.worldCamera, out pos);

        float len = pos.magnitude;
        if (len > max_R)
        {
            pos.x = pos.x * max_R / len;
            pos.y = pos.y * max_R / len;
        }

        stick.localPosition = pos;
    }
}
