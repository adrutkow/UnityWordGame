using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public LetterScript[] hand;
    public Canvas canvas;
    public LetterScript currentSelectedLetter;
    public static PlayerScript playerScript;
    PlayerUIManagerScript playerUIManager;
    public bool selectedLetterThisFrame = false;
    public const int HAND_SIZE = 8;
    public int playerID;
    static int playerIDCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerScript = this;
        playerUIManager = transform.GetChild(0).GetComponent<PlayerUIManagerScript>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        hand = new LetterScript[HAND_SIZE];
        playerID = playerIDCounter;
        playerIDCounter++;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameBoardScript.gameBoard.turn == playerID)
        {
            InputManager();
            playerUIManager.tick();
        }
    }

    public int[] GetMouseTilePosition()
    {
        float ts = GameBoardScript.TILE_SIZE;
        Vector3 offset = new Vector3(ts / 2, ts / 2, 0);
        int x = (int)((Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset) / ts)[0];
        int y = (int)((Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset) / ts)[1];
        return new int[2] { x, y };
    }

    /// <summary>
    /// Change position of player's letters from his hand to the UI's hand.
    /// </summary>
    public void ArrangeHand()
    {
        GameObject canvasHand = GameObject.Find("HandLetters");
        canvasHand.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

        float dist_value = 0.8f * CameraScript.mainCamera.GetComponent<Camera>().orthographicSize / 3.85f;

        //float offset = hand.Length * dist_value;
        //if (hand.Length % 2 == 0) offset += dist_value;
        int emptySlots = 0;
        int s = 0;
        for (int i = 0; i < hand.Length; i++)
        {
            if (hand[i] == null)
            {
                emptySlots++;
                continue;
            }
            hand[i].transform.parent = canvasHand.transform;
            hand[i].transform.position = canvasHand.transform.position + new Vector3(s * dist_value, 0, -1);
            hand[i].transform.localScale = Vector3.one;
            s++;
        }
        float offset = 0;
        if ((hand.Length - emptySlots) % 2 == 0) offset = 0.4f;
        canvasHand.GetComponent<RectTransform>().anchoredPosition -= new Vector2(((hand.Length - emptySlots) / 2) * 0.8f - offset, 0);
    }

    void InputManager()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnLeftClick();
        }

        if (Input.GetMouseButtonDown(1))
        {
            OnRightClick();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ArrangeHand();
            GameBoardScript.gameBoard.DebugInfo();
        }

        if (Input.mouseScrollDelta.y > 0)
        {
            Camera.main.GetComponent<Camera>().orthographicSize += -Input.mouseScrollDelta.y * 0.2f;
        }

        if (Input.mouseScrollDelta.y < 0)
        {
            Camera.main.GetComponent<Camera>().orthographicSize += -Input.mouseScrollDelta.y * 0.2f;
        }
    }

    void OnLeftClick()
    {
        int[] tilePosition = GetMouseTilePosition();
        //Debug.Log(tilePosition[0] + "," + tilePosition[1]);

        RaycastHit2D[] clickedLetters = GetClickedElements("Letter");
        bool UIHasBeenClicked = HasTagBeenClicked("UI");
        bool LetterHasBeenClicked = HasTagBeenClicked("Letter");


        if (!LetterHasBeenClicked)
        {
            if (UIHasBeenClicked && currentSelectedLetter != null)
            {
                currentSelectedLetter.Deselect();
                return;
            }

            if (currentSelectedLetter != null && !UIHasBeenClicked)
            {
                MoveLetter(tilePosition[0], tilePosition[1]);
                currentSelectedLetter.Deselect();
                return;
            }
        }


        if (LetterHasBeenClicked)
        {
            OnClickedLetter(clickedLetters[0].collider.GetComponent<LetterScript>());
        }
    }

    void OnRightClick()
    {
        int[] tilePosition = GetMouseTilePosition();
        GameBoardScript.gameBoard.RemoveLetter(tilePosition[0], tilePosition[1]);
        if (currentSelectedLetter) currentSelectedLetter.Deselect();
    }

    void OnClickedLetter(LetterScript letter)
    {
        if (letter == null) return;

        if (currentSelectedLetter == letter)
        {
            letter.Deselect();
            return;
        }

        if (currentSelectedLetter == null)
        {
            letter.Select();
            return;
        }

        if (currentSelectedLetter != null)
        {
            currentSelectedLetter.Deselect();
            letter.Select();
        }
    }

    public bool HasTagBeenClicked(string tag = null)
    {
        Camera cam = Camera.main;
        RaycastHit2D[] hits = Physics2D.RaycastAll((Vector2)cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hits.Length == 0) return false;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.tag == tag)
            {
                return true;
            }
        }
        return false;
    }

    RaycastHit2D[] GetClickedElements(string tag = null)
    {
        Camera cam = Camera.main;
        RaycastHit2D[] hits = Physics2D.RaycastAll((Vector2)cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (tag == null) return hits;

        List<RaycastHit2D> list = new List<RaycastHit2D>();

        foreach(RaycastHit2D hit in hits)
        {
            if (hit.collider.tag == tag) list.Add(hit);
        }
        return list.ToArray();
    }

    /// <summary>
    /// Move the player's current selected letter. It can be from the hand or from the board itself.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void MoveLetter(int x, int y)
    {
        // Prevent moving permanent letters
        if (currentSelectedLetter.state == LetterScript.State.ON_BOARD_PERMANENTLY) return;

        for (int i = 0; i < hand.Length; i++)
        {
            if (hand[i] == currentSelectedLetter)
            {
                hand[i] = null;
                break;
            }
        }
        GameBoardScript.gameBoard.AddLetter(currentSelectedLetter, x, y);
    }

    /// <summary>
    /// Put a LetterScript object into the player's hand array. If not slot if given, it will try to put it in an empty slot.
    /// </summary>
    /// <param name="letter"></param>
    /// <param name="slot"></param>
    /// <returns>true if action was successful, false otherwise</returns>
    public bool PutLetterInHand(LetterScript letter, int slot=-1)
    {
        // If given slot number is wrong, check if there is an empty slot available.
        if (slot > HAND_SIZE || slot < 0)
        {
            slot = GetEmptyHandSlot();
            if (slot == -1) return false;
        }
        if (hand[slot] != null) return false;

        hand[slot] = letter;
        letter.ChangeState(LetterScript.State.IN_HAND);
        ArrangeHand();
        return true;
    }

    /// <summary>
    /// Find an empty slot in the player's hand, and return its index
    /// </summary>
    /// <returns>the index if found, -1 if no slot is available</returns>
    public int GetEmptyHandSlot()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            if (hand[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Hide the player's hand, currently used for local multiplayer, to not show everyone's hand at once.
    /// </summary>
    public void HideLetters()
    {
        foreach (LetterScript letter in hand)
        {
            if (letter == null) continue;
            letter.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Show the player's hand, currently used for local multiplayer, to not show everyone's hand at once.
    /// </summary>
    public void UnhideLetters()
    {
        foreach (LetterScript letter in hand)
        {
            if (letter == null) continue;
            letter.gameObject.SetActive(true);
        }
    }

    public void FillEmptySpotsInHand()
    {
        for (int i = 0; i < PlayerScript.HAND_SIZE; i++)
        {
            if (hand[i] == null)
            {
                GameBoardScript.gameBoard.GivePlayerLetterFromBag(this);
            }
        }
    }

    public void StartTurn()
    {
        FillEmptySpotsInHand();
        UnhideLetters();
        ArrangeHand();
    }

    public void EndTurn()
    {
        GameBoardScript.gameBoard.isFirstTurn = false;
        HideLetters();
    }

}
