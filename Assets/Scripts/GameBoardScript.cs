using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.SceneTemplate;
using UnityEngine;

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
    public int turn = 0;
    public int starTilePosition = 12;
    public bool isFirstTurn = true;
    public bool isTurnValid = false;
    public int turnScore = 0;
    public List<string> invalidReasons = new List<string>();

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
        starTilePosition = 12;
    }

    void SpawnLetters(int count = 40)
    {
        for (int i = 0; i < count; i++)
        {
            LetterScript newLetter = Instantiate(letterPrefab).GetComponent<LetterScript>();
            newLetter.currentLetter = UnityEngine.Random.Range(0, 12);
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
        isFirstTurn = true;
        foreach(PlayerScript player in playersList)
        {
            for (int i = 0; i < PlayerScript.HAND_SIZE; i++)
            {
                GivePlayerLetterFromBag(player);
            }
            player.HideLetters();
        }
        GetCurrentTurnPlayer().StartTurn();
    }

    public PlayerScript GetCurrentTurnPlayer()
    {
        foreach (PlayerScript player in playersList)
        {
            if (player.playerID == turn) return player;
        }
        return null;
    }

    /// <summary>
    /// Take the first letter from the bag, and give it to the player's hand, if it has an empty slot available
    /// </summary>
    /// <param name="player">true if successful, false otherwise</param>
    public bool GivePlayerLetterFromBag(PlayerScript player)
    {
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

    public int[] GetLetterPosition(LetterScript letter)
    {
        for (int y = 0; y < size[1]; y++)
        {
            for (int x = 0; x < size[0]; x++)
            {
                if (boardLetters[x, y] == letter)
                {
                    return new int[] { x, y };
                }
            }
        }
        return null;
    }

    public void AddLetter(LetterScript letter, int x, int y)
    {
        // idk what i did here, have to fix
        //


        if (boardLetters[x,y] == null)
        {
            int[] oldPos = GetLetterPosition(letter);
            if (oldPos != null) boardLetters[oldPos[0], oldPos[1]] = null;
            boardLetters[x, y] = letter;
            letter.ChangeState(LetterScript.State.ON_BOARD);
            letter.ResetAnimation();
            letter.transform.position = new Vector3(x * TILE_SIZE, y * TILE_SIZE);
            letter.transform.parent = transform;

            letter.transform.localScale = new Vector3(1, 1, 1);
            CheckForWords(x, y);
            if (oldPos != null) CheckForWordsAllAround(oldPos[0], oldPos[1]);
            if (oldPos != null) CheckForWordsAllAround(oldPos[0], oldPos[1]);
            CheckForWordsAllAround(x, y);
            OnBoardChange();
        }

        return;


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
        PlayerScript player = GetCurrentTurnPlayer();
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
            return tempWord;
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

        // If it's the first turn, and the word isn't on a star tile, make it invalid
        if (isFirstTurn)
        {
            if (verticalWord != null)
            {
                if (!verticalWord.IsWordOnStarTile())
                {
                    verticalWord.MakeInvalid("Not on a star tile!");
                }
            }
            if (horizontalWord != null)
            {
                if (!horizontalWord.IsWordOnStarTile())
                {
                    horizontalWord.MakeInvalid("Not on a star tile!");
                }
            }
        }

        if (horizontalWord != null)
        {
            if (!horizontalWord.isStraight())
            {
                horizontalWord.MakeInvalid("Not straight!");
            }
        }

        if (verticalWord != null)
        {
            if (!verticalWord.isStraight())
            {
                verticalWord.MakeInvalid("Not straight!");
            }
        }

        if (horizontalWord != null) AddWordToList(horizontalWord);
        if (verticalWord != null) AddWordToList(verticalWord);
    }

    public bool HasLoneLetters()
    {
        LetterScript letter;
        bool found = false;
        foreach (GameObject letterGO in GameObject.FindGameObjectsWithTag("Letter"))
        {
            letter = letterGO.GetComponent<LetterScript>();
            if (letter.state != LetterScript.State.ON_BOARD) continue;
            Debug.Log("possiblewords:" + possibleWords);

            found = false;
            foreach(Word w in possibleWords)
            {
                if (w.isLetterInWord(letter))
                {
                    found = true;
                    break;
                }
            }
            if (found) continue;
            return true;
        }
        return false;
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
            // TEST: re-check every word for straightness
            // Have to take care of specific cases
            if (word.isStraight())
            {
                if (word.isInvalid) word.MakeValid();
            }
            else
            {
                if (!word.isInvalid) word.MakeInvalid();
            }

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
        UpdateTurn();
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
    /// Updates a turn, checks if the current turn is valid.
    /// </summary>
    public void UpdateTurn()
    {
        isTurnValid = IsTurnValid();
        if (isTurnValid)
        {
            turnScore = CalculateTotalScore();
        }

        if (!isTurnValid)
        {
            foreach (Word w in possibleWords)
            {
                w.MakeInvalid();
            }
        }


    }

    /// <summary>
    /// Checks if a turn can be ended
    /// </summary>
    /// <returns></returns>
    public bool IsTurnValid()
    {
        invalidReasons.Clear();

        if (possibleWords.Count == 0) invalidReasons.Add("No words on board!");
        if (ContainsInvalidWords()) invalidReasons.Add("Invalid words on board!");
        if (HasLoneLetters()) invalidReasons.Add("Lone letters on board!");
        if (isFirstTurn)
        {
            if (possibleWords.Count > 1)
            {
                invalidReasons.Add("First turn must contain only 1 word!");
            }

            if (possibleWords.Count == 1)
            {
                if (!possibleWords[0].IsWordOnStarTile()) invalidReasons.Add("First word must be on a star tile!");
            }
        }
        if (possibleWords.Count > 1)
        {
            Word tempWord = possibleWords[0];
            foreach (Word w in possibleWords)
            {
                if (tempWord == w) continue;
                if (!tempWord.isWordConnected(w))
                {
                    invalidReasons.Add("Disconnected words detected!");
                    break;
                }
            }
        }

        if (invalidReasons.Count > 0) return false;
        return true;
    }

    public bool ContainsInvalidWords()
    {
        foreach (Word word in possibleWords)
        {
            if (word.isInvalid) return true;
        }
        return false;
    }

    /// <summary>
    /// Finish a turn, and place all possibleWords.
    /// </summary>
    public void GameEndTurn()
    {
/*        foreach (Word w in possibleWords)
        {
            w.PlayLockAnimation();
        }*/

        PlaceAllPossibleWords();
        GetCurrentTurnPlayer().EndTurn();


        turnScore = 0;
        isTurnValid = false;
        invalidReasons.Clear();
        turn++;
        if (turn > playersList.Count - 1)
        {
            turn = 0;
        }

        GetCurrentTurnPlayer().StartTurn();
    }

    public void PlaceAllPossibleWords()
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


        PlayerScript player = GetCurrentTurnPlayer();
        /*        for (int i = 0; i < PlayerScript.HAND_SIZE; i++)
                {
                    PutLetterInBag(player.hand[i]);
                    GivePlayerLetterFromBag(player);
                }*/
        player.ArrangeHand();
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
    public bool isInvalid = false;
    public List<string> invalidReasons = new List<string>();
    Color HIGHLIGHT_COLOR = new Color(0.509804f, 0.1826273f, 0.6617778f);
    Color INVALID_HIGHLIGHT_COLOR = Color.gray;

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

    public void MakeInvalid(string reason="Unknown")
    {
        isInvalid = true;
        invalidReasons.Add(reason);
        foreach (GameObject HLtile in highlightTiles)
        {
            HLtile.GetComponent<SpriteRenderer>().color = INVALID_HIGHLIGHT_COLOR;
        }
    }

    public void MakeValid()
    {
        isInvalid = false;
        invalidReasons.Clear();
        foreach (GameObject HLtile in highlightTiles)
        {
            HLtile.GetComponent<SpriteRenderer>().color = HIGHLIGHT_COLOR;
        }
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

    public bool IsConnectedToPermanentLetter()
    {
        if (isHorizontal)
        {
            // If the letter at the beginning of the word is permanent, true
            if (GameBoardScript.gameBoard.boardLetters[x - 1, y] != null)
            {
                if (GameBoardScript.gameBoard.boardLetters[x - 1, y].state == LetterScript.State.ON_BOARD_PERMANENTLY) return true;
            }

            // If the letter at the end of the word is permanent, true
            if (GameBoardScript.gameBoard.boardLetters[x + 1 + length, y] != null)
            {
                if (GameBoardScript.gameBoard.boardLetters[x + 1 + length, y].state == LetterScript.State.ON_BOARD_PERMANENTLY) return true;
            }
        }

        if (!isHorizontal)
        {
            // If the letter above the word is permanent, true
            if (GameBoardScript.gameBoard.boardLetters[x, y + 1] != null)
            {
                if (GameBoardScript.gameBoard.boardLetters[x, y + 1].state == LetterScript.State.ON_BOARD_PERMANENTLY) return true;
            }

            // If the letter at the bottom of the word is permanent, true
            if (GameBoardScript.gameBoard.boardLetters[x, y - 1 - length] != null)
            {
                if (GameBoardScript.gameBoard.boardLetters[x, y - 1 - length].state == LetterScript.State.ON_BOARD_PERMANENTLY) return true;
            }
        }

        return false;
    }

    public bool IsWordOnStarTile()
    {
        int starPos = GameBoardScript.gameBoard.starTilePosition;
        foreach (LetterScript letter in GetAllLetters())
        {
            if (GameBoardScript.gameBoard.boardLetters[starPos, starPos] == letter) return true;
        }
        return false;
    }

    /// <summary>
    /// Method to determine whether the word has attached non-permanent letters to its sides, to prevent "zig-zaggy" turns.
    /// </summary>
    /// <returns></returns>
    public bool isStraight()
    {
        LetterScript scanLetter;
        if (isHorizontal)
        {
            for (int i = 0; i < length; i++)
            {
                scanLetter = GameBoardScript.gameBoard.boardLetters[x + i, y - 1];
                if (scanLetter != null)
                {
                    if (scanLetter.state != LetterScript.State.ON_BOARD_PERMANENTLY)
                    {
                        return false;
                    }
                }

                scanLetter = GameBoardScript.gameBoard.boardLetters[x + i, y + 1];
                if (scanLetter != null)
                {
                    if (scanLetter.state != LetterScript.State.ON_BOARD_PERMANENTLY)
                    {
                        return false;
                    }
                }
            }
        }

        if (!isHorizontal)
        {
            for (int i = 0; i < length; i++)
            {
                scanLetter = GameBoardScript.gameBoard.boardLetters[x + 1, y - i];
                if (scanLetter != null)
                {
                    if (scanLetter.state != LetterScript.State.ON_BOARD_PERMANENTLY)
                    {
                        return false;
                    }
                }

                scanLetter = GameBoardScript.gameBoard.boardLetters[x - 1, y - i];
                if (scanLetter != null)
                {
                    if (scanLetter.state != LetterScript.State.ON_BOARD_PERMANENTLY)
                    {
                        return false;
                    }
                }

            }
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

    public bool isLetterInWord(LetterScript letter)
    {
        Debug.Log(letter.GetLetter() + " is " + letter.GetLetter() + " ?");

        foreach (LetterScript l in GetAllLetters())
        {
            if (ReferenceEquals(letter, l))
            {
                Debug.Log(letter.GetLetter() + " is " + letter.GetLetter());
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if a word has a letter in common with the current word.
    /// </summary>
    /// <returns></returns>
    public bool isWordConnected(Word w)
    {
        List<LetterScript> letterList = GetAllLetters();

        foreach (LetterScript letter in letterList)
        {
            if (letter.state == LetterScript.State.ON_BOARD_PERMANENTLY) continue;
            if (w.isLetterInWord(letter)) return true;
        }
        return false;
    }

    public void PlayLockAnimation()
    {
        foreach (LetterScript l in GetAllLetters())
        {
            l.ResetAnimation();
            //l.GetComponent<Animator>().Play("LockAnimation", 0, 0);
        }
    }

}
