using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.U2D;

public class LetterScript : MonoBehaviour
{
    public Sprite[] letterSprites0;
    public Sprite[] letterSprites1;
    public int currentLetter = 0;
    bool currentSpriteTurn = false;
    public TextMesh textMesh;
    public SpriteRenderer letterSpriteRenderer;
    public bool isSelected;
    public int[] oldPosition = new int[2] { -1, -1 };
    public bool isPermanent = false;
    public State state = State.IN_BAG;

    void Start()
    {
        ChangeLetter(currentLetter);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 180 == 0)
        {
            SwapLetterSprite();
        }
        GetComponent<Animator>().StopPlayback();
        Debug.Log(GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name);
    }

    public void SwapLetterSprite()
    {
        currentSpriteTurn = !currentSpriteTurn;
        if (currentSpriteTurn) letterSpriteRenderer.sprite = letterSprites0[currentLetter];
        if (!currentSpriteTurn) letterSpriteRenderer.sprite = letterSprites1[currentLetter];
    }

    public void ChangeLetter(int v)
    {
        currentLetter = v;
        textMesh.text = Data.values[v].ToString();
        letterSpriteRenderer.sprite = letterSprites0[v];
    }

    public string GetLetter()
    {
        return Data.letters[currentLetter].ToString();
    }

    public void Select()
    {
        if (state == State.IN_BAG) return;

        if (isSelected)
        {
            Deselect();
            return;
        }
        PlayerScript player = GameBoardScript.gameBoard.GetCurrentTurnPlayer();
        isSelected = true;
        if (player.currentSelectedLetter != null) player.currentSelectedLetter.Deselect();
        player.currentSelectedLetter = this;
        GetComponent<SpriteRenderer>().color = Color.green;
        if (isPermanent) GetComponent<SpriteRenderer>().color = Color.yellow;
        player.selectedLetterThisFrame = true;
        PlaySelectAnimation();
    }

    public void PlaySelectAnimation()
    {
        if (state == State.ON_BOARD_PERMANENTLY) return;
        Animator animator = GetComponent<Animator>();
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) return;
        animator.Play("LetterSpin", 0, 0);
    }

    public void ResetAnimation()
    {
        Debug.Log("RESETANIM");
        transform.rotation = new Quaternion(0, 0, 0, 0);
        GetComponent<Animator>().Play("Default", 0, 1);
    }

    public void Deselect()
    {
        isSelected = false;
        PlayerScript player = GameBoardScript.gameBoard.GetCurrentTurnPlayer();
        player.currentSelectedLetter = null;
        GetComponent<SpriteRenderer>().color = Color.white;
        if (isPermanent) GetComponent<SpriteRenderer>().color = Color.magenta;

    }

    public void ToggleSelect()
    {
        if (isSelected)
        {
            Deselect();
            return;
        }
        Select();
    }

    public void MakePermanent()
    {
        state = State.ON_BOARD_PERMANENTLY;
        isPermanent = true;
        GetComponent<SpriteRenderer>().color = Color.magenta;
    }

    public void ChangeState(State s)
    {
        state = s;
    }

    public enum State
    {
        IN_HAND,
        IN_BAG,
        ON_BOARD,
        ON_BOARD_PERMANENTLY
    }

}

