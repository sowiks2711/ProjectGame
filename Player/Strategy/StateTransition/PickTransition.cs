﻿using System;
using System.Collections.Generic;
using System.Text;
using Shared;
using Shared.BoardObjects;
using Shared.GameMessages;
using Shared.GameMessages.PieceActions;

namespace Player.Strategy.StateTransition
{
    class PickTransition : BaseTransition
    {
        public PickTransition(Location location, CommonResources.TeamColour team, int playerId) : base(location, team, playerId)
        { }

        public override GameMessage ExecuteStrategy(Board board)
        {
            ChangeState = PlayerStrategy.PlayerState.Test;
            return new TestPiece
            {
                PlayerId = playerId
            };
        }
    }
}
