using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUIManagerScript : MonoBehaviour
{

    public GameObject placeWordButton;
    public TextMeshProUGUI wordListText;
    public TextMeshProUGUI yourTurnText;
    TextMeshProUGUI placeWordButtonText;

    // Start is called before the first frame update
    void Start()
    {
        placeWordButtonText = placeWordButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void tick()
    {
        placeWordButtonText.text = "";
        // TODO: Bad performance here, to fix later
        if (GameBoardScript.gameBoard.possibleWords.Count != 0)
        {

            bool hasInvalid = false;
            foreach (Word word in GameBoardScript.gameBoard.possibleWords)
            {
                if (word.isInvalid)
                {
                    hasInvalid = true;
                    foreach (string reason in word.invalidReasons)
                    {
                        placeWordButtonText.text += reason;
                    }
                }
            }

            placeWordButton.SetActive(true);
            if (!hasInvalid) placeWordButtonText.text = "Place Word!\n" + GameBoardScript.gameBoard.CalculateTotalScore() + " points";
        } else
        {
            placeWordButton.SetActive(false);
        }

        wordListText.text = "Words so far:\n";
        foreach (string word in GameBoardScript.gameBoard.completedWords)
        {
            wordListText.text += "- " + word + "\n";
        }

        // If it's player0's turn, show "Your turn!" text
        //yourTurnText.gameObject.SetActive(GameBoardScript.gameBoard.turn == 0);

        yourTurnText.text = "Player " + GameBoardScript.gameBoard.turn + "'s turn!";
    }

    public void OnPlaceWordsButtonPressed()
    {
        GameBoardScript.gameBoard.GameEndTurn();
        GameBoardScript.gameBoard.RemoveAllWords(GameBoardScript.gameBoard.possibleWords);
    }
}
