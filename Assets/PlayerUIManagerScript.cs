using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUIManagerScript : MonoBehaviour
{

    public GameObject placeWordButton;
    public TextMeshProUGUI wordListText;
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

        placeWordButtonText.text = "Words so far:\n";

    }

    public void OnPlaceWordsButtonPressed()
    {
        GameBoardScript.gameBoard.PlaceWords();
        GameBoardScript.gameBoard.RemoveAllWords(GameBoardScript.gameBoard.possibleWords);
    }
}
