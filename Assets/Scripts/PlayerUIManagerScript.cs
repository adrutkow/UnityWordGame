using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUIManagerScript : MonoBehaviour
{

    GameObject placeWordButton;
    public TextMeshProUGUI wordListText;
    public TextMeshProUGUI yourTurnText;
    TextMeshProUGUI placeWordButtonText;

    // Start is called before the first frame update
    void Start()
    {
        placeWordButton = GameObject.Find("PlaceWordButton");
        placeWordButtonText = placeWordButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void tick()
    {
        placeWordButtonText.text = "";
        // TODO: Bad performance here, to fix later
        /*if (GameBoardScript.gameBoard.possibleWords.Count != 0)
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
        }*/

        if (GameBoardScript.gameBoard.isTurnValid)
        {
            MakeButtonActive(placeWordButton);
            placeWordButtonText.text = "Place word!\n" + GameBoardScript.gameBoard.turnScore + " points";
        }
        else
        {
            MakeButtonInactive(placeWordButton);
            foreach (string reason in GameBoardScript.gameBoard.invalidReasons)
            {
                placeWordButtonText.text += reason + "\n";
            }
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
        if (!GameBoardScript.gameBoard.isTurnValid) return;
        GameBoardScript.gameBoard.GameEndTurn();
        GameBoardScript.gameBoard.RemoveAllWords(GameBoardScript.gameBoard.possibleWords);
    }

    public void MakeButtonActive(GameObject button)
    {
        button.GetComponent<Animator>().SetBool("Disabled", false);
    }

    public void MakeButtonInactive(GameObject button)
    {
        button.GetComponent<Animator>().SetBool("Disabled", true);
    }
}
