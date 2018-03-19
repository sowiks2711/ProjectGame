﻿using System;
using System.Collections.Generic;
using System.Text;
using Shared;
using Shared.BoardObjects;
using Shared.GameMessages;
using Shared.GameMessages.PieceActions;

namespace Player.Strategy.StateTransition
{
    class TestTransition : BaseTransition
    {
        public TestTransition(Location location, CommonResources.TeamColour team, int playerId, Board board) : base(location, team, playerId, board)
        {
        }

        public override GameMessage ExecuteStrategy()
        {
            var playerInfo = board.Players[playerId];

            if (playerInfo.Piece == null)
            {
                ChangeState = PlayerStrategy.PlayerState.Discover;
                return new Discover
                {
                    PlayerId = playerId
                };
            } else
            {
                ChangeState = PlayerStrategy.PlayerState.MoveToGoalArea;
                var direction = team == CommonResources.TeamColour.Red ? CommonResources.MoveType.Up : CommonResources.MoveType.Down;
                return new Move
                {
                    Direction = direction,
                    PlayerId = playerId
                };
            }

            //switch (playerInfo.Piece.Type)
            //{
            //    case CommonResources.PieceType.Sham:
                    

            //    case CommonResources.PieceType.Normal:
                   

            //    default:
            //        throw new Exception("STH WENT TERRIBLY WRONG");
            //}
        }
    }
}
