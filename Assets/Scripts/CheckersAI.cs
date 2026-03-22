using System.Collections.Generic;
using UnityEngine;

public class CheckersAI
{
    private Player aiPlayer;
    private int searchDepth;

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
        return bestMove;
    }

    private int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer, Player currentTurn)
    {
        List<Moves> possibleMoves = board.GetAllMoves(currentTurn);
        if (depth == 0 || possibleMoves.Count == 0) return EvaluateBoard(board);
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
            int pieceValue = 3;
            int kingValue = 9;
            int value = piece.pieceType == Constants.KING_PIECE ? kingValue : pieceValue;
            int advancement = 0;
            if (piece.pieceType != Constants.KING_PIECE)
            {
                advancement = piece.player == Player.RED ? pos.y : 7 - pos.y;
            }
            int centerBonus = 0;
            if (piece.pieceType == Constants.KING_PIECE)
            {
                int distX = Mathf.Abs(pos.x - 3);
                int distY = Mathf.Abs(pos.y - 3);
                centerBonus = 6 - (distX + distY);
            }
            int totalValue = value + advancement + centerBonus;
            if (piece.player == aiPlayer) score += totalValue;
            else score -= totalValue;
        }
        return score;
    }

    private Player GetOpponent(Player player)
    {
        return player == Player.RED ? Player.BLUE : Player.RED;
    }
}