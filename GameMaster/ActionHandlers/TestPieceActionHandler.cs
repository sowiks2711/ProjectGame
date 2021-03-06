﻿using ClientsCommon.ActionAvailability.AvailabilityChain;
using Common;
using Common.BoardObjects;

namespace GameMaster.ActionHandlers
{
    internal class TestPieceActionHandler : ActionHandler
    {
        public TestPieceActionHandler(int playerId, GameMasterBoard board) : base(playerId, board)
        {
            PlayerId = playerId;
            Board = board;
        }

        protected override bool Validate()
        {
            return new TestAvailabilityChain(PlayerId, Board.Players).ActionAvailable();
        }

        public override BoardData Respond()
        {
            if (!Validate())
                return BoardData.Create(PlayerId, new Piece[0]);

            var player = Board.Players[PlayerId];
            var playerPiece = player.Piece;

            return BoardData.Create(PlayerId, new[] {playerPiece});
        }
    }
}