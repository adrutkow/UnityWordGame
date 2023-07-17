using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneTemplate;
using UnityEngine;

public class GameBoardScript : MonoBehaviour
{
    public static GameBoardScript gameBoard;
    public GameObject tile;
    public GameObject highlightTile;
    public int[] size = new int[2] { 10, 10 };
    public const float TILE_SIZE = 0.77f;
    public TileScript[,] tiles;
    public LetterScript[,] boardLetters;
    public List<Word> possibleWords = new List<Word>();
    public string[] EN_WORDLIST;
    public string[] FR_WORDLIST;
    public string[] wordList;


    void Start()
    {
        gameBoard = this;
        tiles = new TileScript[size[0], size[1]];
        boardLetters = new LetterScript[size[0], size[1]];
        BuildBoard();
        GetWords();
    }


    void GetWords()
    {
        EN_WORDLIST = System.IO.File.ReadAllLines("Assets/en.txt");
        FR_WORDLIST = System.IO.File.ReadAllLines("Assets/fr.txt");
        wordList = FR_WORDLIST;
    }
    void BuildBoard()
    {
        for (int y = 0; y < size[0]; y++)
        {
            for (int x = 0; x < size[1]; x++)
            {
                GameObject temp = Instantiate(tile, new Vector3(x * TILE_SIZE, y * TILE_SIZE), Quaternion.identity);
                temp.GetComponent<TileScript>().SetPosition(x, y);
                tiles[x, y] = temp.GetComponent<TileScript>();
            }
        }
        tiles[12, 12].MakeStarTile();
    }

    public bool IsValidWord(string w)
    {
        /// Check if the given string exists in the current wordList
        /// returns bool
        if (w.Length == 1) return false;
        string testword = w.ToLower();
        foreach (string word in wordList)
        {
            if (testword == word) return true;
        }
        return false;
    }


    public void AddLetter(LetterScript letter, int x, int y)
    {
        letter.transform.position = new Vector3(x * TILE_SIZE, y * TILE_SIZE);
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

    public Word CheckForHorizontalWord(int x, int y)
    {
        /// Checks for a horizontal word at the given position
        /// Returns a Word object if it found a word, null otherwise

        if (boardLetters[x, y] == null) return null;

        string word = boardLetters[x, y].GetLetter();
        int leftMostX = x;

        // Check for letters on the left side
        for (int i = 1; i < 30; i++)
        {
            if (boardLetters[x - i, y] == null)
            {
                break;
            }
            word = boardLetters[x - i, y].GetLetter() + word;
            leftMostX = x - i;
        }

        // Check for letters on the right side
        for (int i = 1; i < 30; i++)
        {
            if (boardLetters[x + i, y] == null)
            {
                break;
            }
            word += boardLetters[x + i, y].GetLetter();
        }

        if (IsValidWord(word))
        {
            Word tempWord = new Word(leftMostX, y, true, word.Length);
            AddWordToList(tempWord);
        }

        return null;
    }

    public Word CheckForVerticalWord(int x, int y)
    {
        /// Checks for a vertical word at the given position
        /// Returns a Word object if it found a word, null otherwise

        if (boardLetters[x, y] == null) return null;

        string word = boardLetters[x, y].GetLetter();
        int upMostY = y;

        // Check for letters on the top side
        for (int i = 1; i < 30; i++)
        {
            if (boardLetters[x, y + i] == null)
            {
                break;
            }

            word = boardLetters[x, y + i].GetLetter() + word;
            upMostY = y + i;
        }

        // Check for letters on the bottom side
        for (int i = 1; i < 30; i++)
        {
            if (boardLetters[x, y - i] == null)
            {
                break;
            }

            word += boardLetters[x, y - i].GetLetter();
        }

        if (IsValidWord(word))
        {
            Word tempWord = new Word(x, upMostY, false, word.Length);
            return tempWord;
        }

        return null;
    }

    public void CheckForWords(int x, int y)
    {
        /// This method scans for horizontal and vertical words at the given position
        /// If it finds words, it adds them to the "possibleWords" list

        if (boardLetters[x, y] == null) return;

        Word horizontalWord = CheckForHorizontalWord(x, y);
        Word verticalWord = CheckForVerticalWord(x, y);

        if (horizontalWord != null) AddWordToList(horizontalWord);
        if (verticalWord != null) AddWordToList(verticalWord);
    }

    public void OnBoardChange()
    {
        /// This method checks every word in the "possibleWords" list
        /// to see if the word has been changed. If so, it removes it from the list
        /// Call this method every time there is a change applied
        /// to the board
        List<Word> toBeRemoved = new List<Word>();
        foreach (Word word in possibleWords)
        {
            // If any of the letters are missing, remove the word
            int ix = 0;
            int iy = 0;
            for (int i = 0; i < word.length; i++)
            {
                if (word.isHorizontal) ix = i;
                if (!word.isHorizontal) iy = i;

                if (boardLetters[word.x + ix, word.y - iy] == null)
                {
                    toBeRemoved.Add(word);
                }
            }

            if (word.isHorizontal)
            {
                // Remove if a letter at the end of the word has been changed (horizontal)
                if (boardLetters[word.x + word.length, word.y] != null)
                {
                    toBeRemoved.Add(word);
                }

                // Remove if a letter at the beginning of the word has been changed (horizontal)
                if (boardLetters[word.x - 1, word.y] != null)
                {
                    toBeRemoved.Add(word);
                }
            }

            if (!word.isHorizontal)
            {
                // Remove if a letter at the end of the word has been changed (vertical)
                if (boardLetters[word.x, word.y - word.length] != null)
                {
                    toBeRemoved.Add(word);
                }

                // Remove if a letter at the beginning of the word has been changed (vertical)
                if (boardLetters[word.x, word.y + 1] != null)
                {
                    toBeRemoved.Add(word);
                }
            }
        }
        RemoveWordsFromListOfWords(possibleWords, toBeRemoved);
    }

    public void AddWordToList(Word w)
    {
        /// Add a Word object to the "possibleWords" list and highlight it
        possibleWords.Add(w);
        w.Hightlight();
    }

    public void RemoveWordsFromListOfWords(List<Word> originalList, List<Word> wordsToBeRemoved)
    {
        foreach (Word w in wordsToBeRemoved)
        {
            w.UnHighlight();
            originalList.Remove(w);
            CheckForWords(w.x, w.y);
        }
    }

    public void RemoveAllWords(List<Word> originalList)
    {
        List<Word> listCopy = new List<Word>();
        for (int i = 0; i < originalList.Count; i++)
        {
            listCopy.Add(originalList[i]);
        }
        RemoveWordsFromListOfWords(originalList, listCopy);
    }


    public void PlaceWords()
    {
        foreach (Word word in possibleWords)
        {
            word.MakePermanent();
        }
        RemoveAllWords(possibleWords);
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
        for (int i = 0; i < length; i++)
        {
            GameObject temp = GameObject.Instantiate(GameBoardScript.gameBoard.highlightTile);

            float ts = GameBoardScript.TILE_SIZE;
            if (isHorizontal) temp.transform.position = new Vector3((x + i) * ts, y * ts);
            if (!isHorizontal) temp.transform.position = new Vector3(x * ts, (y - i) * ts);

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

    public List<LetterScript> GetAllLetters()
    {
        List<LetterScript> temp = new List<LetterScript>();
        int ix = 0;
        int iy = 0;

        for (int i = 0; i < length; i++)
        {
            if (isHorizontal) ix = i;
            if (!isHorizontal) iy = i;
            temp.Add(GameBoardScript.gameBoard.boardLetters[x + ix, y - iy]);
        }

        return temp;
    }

    public void MakePermanent()
    {
        foreach (LetterScript letter in GetAllLetters())
        {
            letter.MakePermanent();
        }
    }

}
