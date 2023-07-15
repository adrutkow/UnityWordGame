using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUIManagerScript : MonoBehaviour
{

    public GameObject placeWordButton;
    TextMeshProUGUI placeWordButtonText;

    // Start is called before the first frame update
    void Start()
    {
        placeWordButtonText = placeWordButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        tick();
    }

    public void tick()
    {
        if (GameBoardScript.gameBoard.possibleWords.Count != 0)
        {
            placeWordButton.SetActive(true);
        } else
        {
            placeWordButton.SetActive(false);
        }
    }
}
