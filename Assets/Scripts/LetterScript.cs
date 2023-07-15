using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;
using UnityEngine.U2D;

public class LetterScript : MonoBehaviour
{
    public Sprite[] letterSprites0;
    public Sprite[] letterSprites1;
    public int currentLetter = 0;
    public TextMesh textMesh;
    public SpriteRenderer letterSpriteRenderer;
    public bool isSelected;
    public int[] oldPosition = new int[2] {-1, -1};
    public bool isPermanent = false;

    // Start is called before the first frame update
    void Start()
    {
        ChangeLetter(currentLetter);
    }

    // Update is called once per frame
    void Update()
    {
        
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
        if (isSelected)
        {
            Deselect();
            return;
        }
        PlayerScript player = GameObject.Find("Player").GetComponent<PlayerScript>();
        isSelected = true;
        if (player.currentSelectedLetter != null) player.currentSelectedLetter.Deselect();
        player.currentSelectedLetter = this;
        GetComponent<SpriteRenderer>().color = Color.green;
        if (isPermanent) GetComponent<SpriteRenderer>().color = Color.yellow;
        player.selectedLetterThisFrame = true;
    }

    public void Deselect()
    {
        isSelected = false;
        PlayerScript player = GameObject.Find("Player").GetComponent<PlayerScript>();
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
        isPermanent = true;
        GetComponent<SpriteRenderer>().color = Color.magenta;

    }

}
