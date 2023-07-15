using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public GameObject starTileIcon;
    public bool isStarTile = false;
    int[] position = new int[2];


    // Start is called before the first frame update
    void Start()
    {
        
        if (isStarTile)
        {
            MakeStarTile();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPosition(int x, int y)
    {
        position[0] = x;
        position[1] = y;
    }

    public void MakeStarTile()
    {
        starTileIcon.SetActive(true);
        isStarTile = true;
    }
}
