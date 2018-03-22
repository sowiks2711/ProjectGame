﻿using System.Collections;
using System.Collections.Generic;
using Common.BoardObjects;

namespace Common.Interfaces
{
    public interface IBoard : IEnumerable
    {
        Dictionary<int, PlayerInfo> Players { get; }
        Dictionary<int, Piece> Pieces { get; }

        int TaskAreaSize { get; }
        int GoalAreaSize { get; }
        int Width { get; }
        int Height { get; }
    }
}