using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameBoardScript : MonoBehaviour
{
    public static GameBoardScript gameBoard;
    public GameObject tile;
    public GameObject highlightTile;
    public int[] size = new int[2] { 10, 10 };
    public TileScript[,] tiles;
    public LetterScript[,] boardLetters;
    public List<Word> possibleWords = new List<Word>();
    public string[] EN_WORDLIST;
    public string[] FR_WORDLIST;
    public string[] wordList;



    // Start is called before the first frame update
    void Start()
    {
        gameBoard = this;
        tiles = new TileScript[size[0], size[1]];
        boardLetters = new LetterScript[size[0], size[1]];
        BuildBoard();
        GetWords();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetWords()
    {
        EN_WORDLIST = System.IO.File.ReadAllLines("Assets/en.txt");
        FR_WORDLIST = System.IO.File.ReadAllLines("Assets/fr.txt");
        wordList = FR_WORDLIST;
    }

    public bool CheckIfWordExists(string w)
    {
        if (w.Length == 1) return false;
        string testword = w.ToLower();
        foreach (string word in wordList)
        {
            if (testword == word) return true;
        }
        return false;
    }

    void BuildBoard()
    {
        for (int y = 0; y < size[0]; y++)
        {
            for (int x = 0; x < size[1]; x++)
            {
                GameObject temp = Instantiate(tile, new Vector3(x * 0.77f, y * 0.77f), Quaternion.identity);
                temp.GetComponent<TileScript>().SetPosition(x, y);
                tiles[x,y] = temp.GetComponent<TileScript>();
            }
        }
        tiles[12, 12].MakeStarTile();
    }

    public void AddLetter(LetterScript letter, int x, int y)
    {
        letter.transform.position = new Vector3(x * 0.77f, y * 0.77f);
        letter.transform.parent = transform;

        if (letter.oldPosition[0] != -1)
        {
            boardLetters[letter.oldPosition[0], letter.oldPosition[1]] = null;
            CheckForWordsAllAround(letter.oldPosition[0], letter.oldPosition[1]);
        }

        letter.oldPosition[0] = x;
        letter.oldPosition[1] = y;

        boardLetters[x, y] = letter;
        CheckForWords(x, y);
        CheckForWordsAllAround(x, y);
        OnBoardChange();
    }

    public void RemoveLetter(int x, int y)
    {
        LetterScript letter = boardLetters[x, y];
        boardLetters[x, y] = null;
        if (letter == null) return;

        PlayerScript player = GameObject.Find("Player").GetComponent<PlayerScript>();


        for (int i = 0; i < player.hand.Length; i++)
        {
            if (player.hand[i] == null)
            {
                print("empty");
                player.hand[i] = letter.gameObject;
                break;
            }
        }
        player.ArrangeHand();
        CheckForWordsAllAround(x, y);
        OnBoardChange();
    }

    public void CheckForWordsAllAround(int x, int y)
    {
        CheckForWords(x - 1, y);
        CheckForWords(x + 1, y);
        CheckForWords(x, y + 1);
        CheckForWords(x, y - 1);
    }

    public void CheckForWords(int x, int y)
    {
        if (boardLetters[x, y] == null)
        {
            Debug.Log("No letter here");
            return;
        }

        string tempH = boardLetters[x, y].GetLetter();
        string tempV = boardLetters[x, y].GetLetter();

        int leftMostX = x;
        int upMostY = y;


        // Check for letters on the left side
        for (int i = 1; i < 30; i++)
        {
            if (boardLetters[x - i, y] == null)
            {
                break;
            }

            tempH = boardLetters[x - i, y].GetLetter() + tempH;
            leftMostX = x - i;
        }

        // Check for letters on the right side
        for (int i = 1; i < 30; i++)
        {
            if (boardLetters[x + i, y] == null)
            {
                break;
            }

            tempH += boardLetters[x + i, y].GetLetter();
        }

        Debug.Log("Horizontal word is: " + tempH);
        Debug.Log("Is word?: " + CheckIfWordExists(tempH).ToString());

        if (CheckIfWordExists(tempH))
        {
            Word tempWord = new Word(leftMostX, y, true, tempH.Length);
            Debug.Log(tempWord);
            possibleWords.Add(tempWord);
            tempWord.Hightlight();
        }


        // Check for letters on the top side
        for (int i = 1; i < 30; i++)
        {
            if (boardLetters[x, y + i] == null)
            {
                break;
            }

            tempV = boardLetters[x, y + i].GetLetter() + tempV;
            upMostY = y + i;
        }

        // Check for letters on the bottom side
        for (int i = 1; i < 30; i++)
        {
            if (boardLetters[x, y - i] == null)
            {
                break;
            }

            tempV += boardLetters[x, y - i].GetLetter();
        }

        Debug.Log("Vertical word is: " + tempV);
        Debug.Log("Is word?: " + CheckIfWordExists(tempV).ToString());

        if (CheckIfWordExists(tempV))
        {
            Word tempWord = new Word(x, upMostY, false, tempV.Length);
            Debug.Log(tempWord);
            possibleWords.Add(tempWord);
            tempWord.Hightlight();
        }
    }

    public void OnBoardChange()
    {
        Debug.Log("Testing for words");
        List<Word> toBeRemoved = new List<Word>();
        foreach (Word word in possibleWords)
        {
            int ix = 0;
            int iy = 0;
            for (int i = 0; i < word.length; i++)
            {
                if (word.isHorizontal) ix = i;
                if (!word.isHorizontal) iy = i;

                if (boardLetters[word.x + ix, word.y - iy] == null)
                {
                    word.UnHighlight();
                    toBeRemoved.Add(word);
                }
            }

            if (boardLetters[word.x + word.length,word.y] != null)
            {
                word.UnHighlight();
                toBeRemoved.Add(word);
            }

            if (boardLetters[word.x - 1, word.y] != null)
            {
                word.UnHighlight();
                toBeRemoved.Add(word);
            }
        }


        foreach (Word w in toBeRemoved)
        {
            possibleWords.Remove(w);
            CheckForWords(w.x, w.y);
            Debug.Log("Removed highlight and word");
        }

    }

}


public class Word
{
    public int x;
    public int y;
    public bool isHorizontal;
    public int length;
    public List<GameObject> highlightTiles = new List<GameObject>();

    public Word(int _x, int _y, bool _isHorizontal, int _length)
    {
        x = _x;
        y = _y;
        isHorizontal = _isHorizontal;
        length = _length;
    }

    public void Hightlight()
    {

        //letter.transform.position = new Vector3(x * 0.77f, y * 0.77f);
        for (int i = 0; i < length; i++)
        {
            GameObject temp = GameObject.Instantiate(GameBoardScript.gameBoard.highlightTile);

            if (isHorizontal) temp.transform.position = new Vector3((x + i) * 0.77f, y * 0.77f);
            if (!isHorizontal) temp.transform.position = new Vector3(x * 0.77f, (y - i) * 0.77f);

            highlightTiles.Add(temp);
        }
    }

    public void UnHighlight()
    {
        for (int i = 0; i < highlightTiles.Count; i++)
        {
            GameObject.Destroy(highlightTiles[i]);
        }
    }

}
