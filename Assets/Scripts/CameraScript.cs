using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    Vector3 difference;
    Vector3 origin;
    Camera cam;
    bool drag = false;
    public bool firstClickedNothing = false;
    PlayerScript player;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        player = GameObject.Find("Player").GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            firstClickedNothing = !player.HasTagBeenClicked("UI") & !player.HasTagBeenClicked("Letter");
        }


        if (Input.GetMouseButton(0) && firstClickedNothing)
        {
            difference = cam.ScreenToWorldPoint(Input.mousePosition) - cam.transform.position;
            if (drag == false)
            {
                drag = true;
                origin = cam.ScreenToWorldPoint(Input.mousePosition);
            }

        } else
        {
            drag = false;
        }

        if (drag) transform.position = origin - difference;
    }
}
