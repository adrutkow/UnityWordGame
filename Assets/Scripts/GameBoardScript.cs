using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class GameBoardScript : MonoBehaviour
{
    public static GameBoardScript gameBoard;
    public GameObject letterPrefab;
    public GameObject tile;
    public GameObject highlightTile;
    public int[] size = new int[2] { 10, 10 };
    public const float TILE_SIZE = 0.77f;
    public TileScript[,] tiles;
    public LetterScript[,] boardLetters;
    public List<Word> possibleWords = new List<Word>();
    public List<string> completedWords = new List<string>();
    public string[] EN_WORDLIST;
    public string[] FR_WORDLIST;
    public string[] wordList;
    public List<LetterScript> letterBag;
    public List<PlayerScript> playersList;


    void Start()
    {
        gameBoard = this;
        tiles = new TileScript[size[0], size[1]];
        boardLetters = new LetterScript[size[0], size[1]];
        letterBag = new List<LetterScript>();

        BuildBoard();
        GetWords();
        SpawnLetters();
        GetPlayers();
        RoundStart();
    }

    /// <summary>
    /// Change the current wordList to a certain language. Defaults to English.
    /// Valid options: "EN", "FR"
    /// </summary>
    /// <param name="language"></param>
    void GetWords(string language="EN")
    {
        EN_WORDLIST = System.IO.File.ReadAllLines("Assets/en.txt");
        FR_WORDLIST = System.IO.File.ReadAllLines("Assets/fr.txt");
        wordList = EN_WORDLIST;
        if (language == "FR") wordList = FR_WORDLIST;
    }

    /// <summary>
    /// Place a bunch of tiles to represent the board, decorative.
    /// </summary>
    void BuildBoard()
    {
        GameObject bgTiles = GameObject.Find("BackgroundTiles");
        for (int y = 0; y < size[0]; y++)
        {
            for (int x = 0; x < size[1]; x++)
            {
                GameObject temp = Instantiate(tile, new Vector3(x * TILE_SIZE, y * TILE_SIZE), Quaternion.identity);
                temp.GetComponent<TileScript>().SetPosition(x, y);
                temp.transform.parent = bgTiles.transform;
                tiles[x, y] = temp.GetComponent<TileScript>();
            }
        }
        tiles[12, 12].MakeStarTile();
    }

    void SpawnLetters(int count = 40)
    {
        for (int i = 0; i < count; i++)
        {
            LetterScript newLetter = Instantiate(letterPrefab).GetComponent<LetterScript>();
            newLetter.currentLetter = Random.Range(0, 26);
            letterBag.Add(newLetter);
        }
    }

    void GetPlayers()
    {
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            playersList.Add(g.GetComponent<PlayerScript>());
        }
    }

    void RoundStart()
    {
        foreach(PlayerScript player in playersList)
        {
            for (int i = 0; i < PlayerScript.HAND_SIZE; i++)
            {
                GivePlayerLetterFromBag(player);
            }
        }
    }

    /// <summary>
    /// Take the first letter from the bag, and give it to the player's hand, if it has an empty slot available
    /// </summary>
    /// <param name="player">true if successful, false otherwise</param>
    public bool GivePlayerLetterFromBag(PlayerScript player)
    {
        player = GameObject.Find("Player").GetComponent<PlayerScript>();
        if (letterBag.Count == 0) return false;

        for (int i = 0; i < PlayerScript.HAND_SIZE; i++)
        {
            if (player.hand[i] == null)
            {
                LetterScript letterToGive = letterBag[0];
                if (player.PutLetterInHand(letterToGive, i))
                {
                    letterBag.RemoveAt(0);
                    return true;
                }
            }
        }
        return false;
    }

    public void PutLetterInBag(LetterScript letter)
    {
        if (letter == null) return;
        if (!(letter.state == LetterScript.State.IN_HAND)) return;

        for (int i = 0; i < PlayerScript.HAND_SIZE; i++)
        {
            if (letter == playersList[0].hand[i])
            {
                playersList[0].hand[i] = null;
                letterBag.Add(letter);
                letter.state = LetterScript.State.IN_BAG;
                letter.gameObject.transform.position = new Vector3(-100, -100);
                return;
            }
        }

    }


    /// <summary>
    /// Checks if the given string exists in the current wordList.
    /// </summary>
    /// <param name="w"></param>
    /// <returns>True if it exists, False if not.</returns>
    public bool IsValidWord(string w)
    {

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
        // idk what i did here, have to fix
        letter.ResetAnimation();
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
        letter.ChangeState(LetterScript.State.ON_BOARD);
        CheckForWords(x, y);
        CheckForWordsAllAround(x, y);
        OnBoardChange();
    }

    /// <summary>
    /// Remove a letter at given position, and put it in the player's bag. This is only for
    /// temporary letters, that a player is placing during his turn.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void RemoveLetter(int x, int y)
    {
        LetterScript letter = boardLetters[x, y];
        if (letter == null) return;
        if (!(letter.state == LetterScript.State.ON_BOARD)) return;

        letter.ResetAnimation();
        PlayerScript player = playersList[0];
        if (player.PutLetterInHand(letter))
        {
            boardLetters[x, y] = null;
            CheckForWordsAllAround(x, y);
            OnBoardChange();
        }
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

    /// <summary>
    /// Checks for a vertical word at the given position
    /// Returns a Word object if it found a word, null otherwise
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Word CheckForVerticalWord(int x, int y)
    {
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

    /// <summary>
    /// This method scans for horizontal and vertical words at the given position
    /// If it finds words, it adds them to the "possibleWords" list
    /// It doesn't add if all letters are already permanent letters.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void CheckForWords(int x, int y)
    {
        if (boardLetters[x, y] == null) return;

        Word horizontalWord = CheckForHorizontalWord(x, y);
        Word verticalWord = CheckForVerticalWord(x, y);

        if (horizontalWord != null) AddWordToList(horizontalWord);
        if (verticalWord != null) AddWordToList(verticalWord);
    }

    /// <summary>
    /// This method checks every word in the "possibleWords" list
    /// to see if the word has been changed. If so, it removes it from the list.
    /// Call this method every time there is a change applied
    /// to the board
    /// </summary>
    public void OnBoardChange()
    {

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

    /// <summary>
    /// Add a Word object to the "possibleWords" list, only if it's not made of only permanent letters.
    /// </summary>
    /// <param name="w"></param>
    public void AddWordToList(Word w)
    {
        if (w.IsMadeOfPermanentLetters()) return;
        if (possibleWords.Contains(w)) return;

        foreach (Word word in possibleWords)
        {
            if (word.GetWordString() == w.GetWordString()) return;
        }

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

    /// <summary>
    /// Remove all possibleWords from a given list of words
    /// </summary>
    /// <param name="originalList"></param>
    public void RemoveAllWords(List<Word> originalList)
    {
        List<Word> listCopy = new List<Word>();
        for (int i = 0; i < originalList.Count; i++)
        {
            listCopy.Add(originalList[i]);
        }
        RemoveWordsFromListOfWords(originalList, listCopy);
    }

    /// <summary>
    /// Finish a turn, and place all possibleWords.
    /// </summary>
    public void PlaceWords()
    {
        foreach (Word word in possibleWords)
        {
            word.MakePermanent();
            completedWords.Add(word.GetWordString());
        }
        RemoveAllWords(possibleWords);
    }

    public void DebugInfo()
    {
        Debug.Log("possibleWords:"+possibleWords.Count);
        foreach(Word word in possibleWords)
        {
            Debug.Log(word.GetWordString());
        }

        PlayerScript player = playersList[0];
        for (int i = 0; i < PlayerScript.HAND_SIZE; i++)
        {
            PutLetterInBag(player.hand[i]);
            GivePlayerLetterFromBag(player);
        }

    }

    public int CalculateTotalScore()
    {
        int totalScore = 0;
        foreach (Word w in possibleWords)
        {
            totalScore += w.CalculateScore();
        }
        return totalScore;
    }

}

/// <summary>
/// A Word object, it represents a chain of letter objects, connected either horizontally or vertically.
/// </summary>
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

    public bool IsMadeOfPermanentLetters()
    {
        foreach (LetterScript letter in GetAllLetters())
        {
            if (!(letter.state == LetterScript.State.ON_BOARD_PERMANENTLY)) return false;
        }
        return true;
    }

    /// <summary>
    /// Get the word's content, the word itself as a string
    /// </summary>
    /// <returns>The word as a string</returns>
    public string GetWordString()
    {
        string tempString = "";
        foreach(LetterScript letter in GetAllLetters())
        {
            if (letter == null) return "";
            tempString += letter.GetLetter();
        }
        return tempString;
    }

    public int CalculateScore()
    {
        int score = 0;
        foreach (LetterScript letter in GetAllLetters())
        {
            score += Data.values[letter.currentLetter];
        }
        return score;
    }

}
