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
        // TODO: Bad performance here, to fix later
        if (GameBoardScript.gameBoard.possibleWords.Count != 0)
        {
            placeWordButton.SetActive(true);
            placeWordButtonText.text = "Place Word!\n" + GameBoardScript.gameBoard.CalculateTotalScore() + " points";
        } else
        {
            placeWordButton.SetActive(false);
        }

        wordListText.text = "Words so far:\n";
        foreach (string word in GameBoardScript.gameBoard.completedWords)
        {
            wordListText.text += "- " + word + "\n";
        }

    }

    public void OnPlaceWordsButtonPressed()
    {
        GameBoardScript.gameBoard.PlaceWords();
        GameBoardScript.gameBoard.RemoveAllWords(GameBoardScript.gameBoard.possibleWords);
    }
}
