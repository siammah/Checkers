using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject block;
    [SerializeField] GameObject piece;
    bool hasGameFinished, canMove;
    string gameState;
    Player currentPlayer;
    Dictionary<GamePiece, GameObject> pieceDictionary;
    GamePiece clickedPiece;
    Board myBoard;
    public static GameManager instance;
    public delegate void UpdateMessage(Player player, string temp);
    public event UpdateMessage Message;
    bool hasAIMoved = false;
    int depth = 8;
    CheckersAI AI;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        SpawnBlocks();
        myBoard = new Board();
        gameState = Constants.AI_TURN;
        canMove = false;
        hasGameFinished = false;
        currentPlayer = Player.RED;
        pieceDictionary = new Dictionary<GamePiece, GameObject>();
        Dictionary<GamePiece, Grid> posGrid = myBoard.playerPositions;
        foreach (KeyValuePair<GamePiece, Grid> pair in posGrid)
        {
            GameObject pieceObject = Instantiate(piece);
            pieceObject.transform.position = new Vector3(pair.Value.x, -pair.Value.y, -2f);
            pieceObject.GetComponent<SpriteRenderer>().color = pair.Key.player == Player.RED ? Color.red : Color.blue;
            pieceDictionary[pair.Key] = pieceObject;
        }

        AI = new CheckersAI(Player.RED, depth);
    }

    void SpawnBlocks()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject temp = Instantiate(block);
                temp.transform.position = new Vector3(i, -j, -1f);
                temp.GetComponent<SpriteRenderer>().color = (i + j) % 2 == 0 ? Color.grey : Color.black;
            }
        }
    }

    void Update()
    {
        if (hasGameFinished) return;
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Grid clickedGrid = new Grid() { x = (int)(mousePos.x + 0.5), y = (int)-(mousePos.y - 0.5) };
            switch (gameState)
            {
                case Constants.CLICK:
                    canMove = false;
                    clickedPiece = myBoard.GetPieceAtGrid(clickedGrid);
                    myBoard.CalculateMoves(currentPlayer);
                    var moveDictionary = myBoard.playerMoves;
                    if (moveDictionary.Count == 0)
                    {
                        hasGameFinished = true;
                        Message(currentPlayer, Constants.FINISHED);
                    }
                    if (clickedPiece.player != currentPlayer || clickedPiece.pieceNumber == -1) return;
                    foreach (var item in moveDictionary)
                    {
                        if (item.Key == clickedPiece) canMove = true;
                    }
                    if (!canMove) return;
                    Message(currentPlayer, Constants.MOVE);
                    gameState = Constants.MOVE;
                    break;

                case Constants.MOVE:
                    List<Moves> moves = myBoard.playerMoves[clickedPiece];
                    foreach (Moves currentMove in moves)
                    {
                        if (currentMove.end.x == clickedGrid.x && currentMove.end.y == clickedGrid.y)
                        {
                            pieceDictionary[clickedPiece].transform.position = new Vector3(clickedGrid.x, -clickedGrid.y, -2f);
                            if (currentMove.isCapture)
                            {
                                pieceDictionary[currentMove.capturedPiece].SetActive(false);
                                pieceDictionary.Remove(currentMove.capturedPiece);
                            }
                            myBoard.UpdateMove(currentMove);
                            if (currentMove.end.y == 0 && currentPlayer == Player.BLUE)
                            {
                                myBoard.UpgradePiece(clickedPiece);
                                pieceDictionary[clickedPiece].transform.GetChild(0).gameObject.SetActive(true);
                            }
                            gameState = Constants.CLICK;
                            myBoard.CalculateMoves(currentPlayer);
                            if (myBoard.isCapturedMove && currentMove.isCapture)
                            {
                                Message(currentPlayer, Constants.CLICK);
                                return;
                            }
                            currentPlayer = Player.RED;
                            gameState = Constants.AI_TURN;
                            hasAIMoved = false;

                            
                            Message(currentPlayer, Constants.CLICK);
                            return;
                        }
                    }
                    break;
            }
        }
        if (gameState.Equals(Constants.AI_TURN) && !hasAIMoved)
        {
            hasAIMoved = true;
            Moves bestMove = AI.GetBestMove(myBoard);
            GamePiece key = new GamePiece(Player.RED, 100);
            if (bestMove.start.x == 0 && bestMove.end.x == 0){
                hasGameFinished = true;
                return;
            }
            foreach (var g in myBoard.playerPositions)
            {
                if (g.Value.x == bestMove.start.x && g.Value.y == bestMove.start.y)
                {
                    key = g.Key;
                    break;
                }
            }
            if (bestMove.isCapture)
            {
                pieceDictionary[bestMove.capturedPiece].SetActive(false);
                pieceDictionary.Remove(bestMove.capturedPiece);
            }

            myBoard.UpdateMove(bestMove);
            pieceDictionary[key].transform.position = new Vector3(bestMove.end.x, -bestMove.end.y, -2f);

            if (bestMove.end.y == 7 && currentPlayer == Player.RED) //BRING THIS TO AI SECTION LATER
            {
                myBoard.UpgradePiece(key);
                pieceDictionary[key].transform.GetChild(0).gameObject.SetActive(true);
            }

            gameState = Constants.CLICK;
            currentPlayer = Player.BLUE;
            Message(currentPlayer, Constants.CLICK);
        }
    }

    public void GameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void GameRestart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}