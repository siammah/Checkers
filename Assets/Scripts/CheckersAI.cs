using System.Collections.Generic;
using UnityEngine;

public class CheckersAI
{
    private Player aiPlayer;
    private int searchDepth;
    public static Dictionary<char[], int> boardStorage = new Dictionary<char[], int>();

    public CheckersAI(Player aiPlayer, int depth)
    {
        this.aiPlayer = aiPlayer;
        this.searchDepth = depth;
    }

    public Moves GetBestMove(Board board)
    {
        List<Moves> possibleMoves = board.GetAllMoves(aiPlayer);
        if (possibleMoves.Count == 0) return new Moves();
        int bestScore = int.MinValue;
        Moves bestMove = possibleMoves[0];
        foreach (Moves move in possibleMoves)
        {
            Board newBoard = new Board(board);
            newBoard.SimulateMove(move);
            int score = Minimax(newBoard, searchDepth - 1, int.MinValue, int.MaxValue, false, GetOpponent(aiPlayer));
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        foreach (Moves move in possibleMoves)
        {
            if(!move.Equals(bestMove))
            {
                Board newBoard = new Board(board);
                newBoard.SimulateMove(move);
                RemoveDictEntries(searchDepth - 1, newBoard, GetOpponent(aiPlayer));
            }
        }
        return bestMove;
    }

    private int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer, Player currentTurn)
    {
        List<Moves> possibleMoves = board.GetAllMoves(currentTurn);
        if (depth == 0 || possibleMoves.Count == 0) return EvaluateBoard(board);


        //Dictionary Stuff
        DictBoard check = new DictBoard(board);
        if (boardStorage.ContainsKey(check.board))
            return boardStorage[check.board];

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (Moves move in possibleMoves)
            {
                Board newBoard = new Board(board);
                newBoard.SimulateMove(move);
                int eval = Minimax(newBoard, depth - 1, alpha, beta, false, GetOpponent(currentTurn));
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                if (beta <= alpha) break;
            }
            boardStorage.Add(check.board, maxEval);  //Dictionary Stuff
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (Moves move in possibleMoves)
            {
                Board newBoard = new Board(board);
                newBoard.SimulateMove(move);
                int eval = Minimax(newBoard, depth - 1, alpha, beta, true, GetOpponent(currentTurn));
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                if (beta <= alpha) break;
            }
            boardStorage.Add(check.board, minEval); //Dictionary Stuff
            return minEval;
        }
    }

    private int EvaluateBoard(Board board)
    {
        int score = 0;

        foreach (var item in board.playerPositions)
        {
            GamePiece piece = item.Key;
            Grid pos = item.Value;

            int pieceValue = 12;
            int kingValue = 20;
            int value = piece.pieceType == Constants.KING_PIECE ? kingValue : pieceValue;

            int advancement = 0;
            if (piece.pieceType != Constants.KING_PIECE)
            {
                int distToKing = piece.player == Player.RED ? (7 - pos.y) : pos.y;
                advancement = (6 - distToKing);
            }

            int distX = Mathf.Abs(pos.x - 3);
            int centerBonus = Mathf.Min(1, 3 - Mathf.Min(distX, 3));

            int edgePenalty = (pos.x == 0 || pos.x == 7) ? -1 : 0;

            int kingSafetyPenalty = 0;
            if (piece.pieceType == Constants.KING_PIECE && (pos.y == 0 || pos.y == 7))
                kingSafetyPenalty = -2;

            int totalValue = value + advancement + centerBonus + edgePenalty + kingSafetyPenalty;

            if (piece.player == aiPlayer)
                score += totalValue;
            else
                score -= totalValue;
        }

        return score;
    }

    private Player GetOpponent(Player player)
    {
        return player == Player.RED ? Player.BLUE : Player.RED;
    }

    public void RemoveDictEntries(int depth, Board board, Player currentTurn)
    {
        if (depth == 0) return;
        List<Moves> possibleMoves = board.GetAllMoves(currentTurn);
        foreach (Moves move in possibleMoves)
        {
            Board newBoard = new Board(board);
            newBoard.SimulateMove(move);
            RemoveDictEntries(depth - 1, newBoard, GetOpponent(currentTurn));
        }
        DictBoard dictentry = new DictBoard(board);
        if (boardStorage.ContainsKey(dictentry.board))
            boardStorage.Remove(dictentry.board);
    }
}