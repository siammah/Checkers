using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictBoard
{

    public char[] board = new char[32];
    // Start is called before the first frame update
    public DictBoard(Board b)
    {
        foreach (var item in b.playerPositions)
        {
            board[GetPiecePos(item.Value)] = GetPieceType(item.Key);
        }
    }

    public int GetPiecePos(Grid g)
    {
        return g.x * 4 + g.y;
    }
    public char GetPieceType(GamePiece p)
    {
        bool isKing = !p.pieceType.Equals(Constants.NORMAL_PIECE);
        Player type = p.player;
        if (type == Player.RED)
        {
            if (isKing)
                return 'R';
            return 'r';
        }
        if (isKing)
            return 'B';
        return 'b';
    }

    public bool Equals(DictBoard other)
    {
        for (int i = 0; i < board.Length; i++)
        {
            if (other.board[i] != board[i])
                return false;
        }
        return true;
    }

    public bool Equals(char[] other)
    {
        for (int i = 0; i < board.Length; i++)
        {
            if (other[i] != board[i])
                return false;
        }
        return true;
    }
}
