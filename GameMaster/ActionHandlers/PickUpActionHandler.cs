﻿using ClientsCommon.ActionAvailability.AvailabilityChain;
using Common;
using Common.BoardObjects;

namespace GameMaster.ActionHandlers
{
    internal class PickUpActionHandler : ActionHandler
    {
        public PickUpActionHandler(int playerId, GameMasterBoard board) : base(playerId, board)
        {
            PlayerId = playerId;
            Board = board;
        }

        protected override bool Validate()
        {
            var playerInfo = Board.Players[PlayerId];
            return new PickUpAvailabilityChain(playerInfo.Location, Board, PlayerId).ActionAvailable();
        }

        public override BoardData Respond()
        {
            if (!Validate())
                return BoardData.Create(PlayerId, new Piece[0]);

            var player = Board.Players[PlayerId];
            var playerField = Board[player.Location] as TaskField;

            var piece = Board.Pieces[playerField.PieceId.Value];
            piece.PlayerId = PlayerId;

            player.Piece = piece;
            playerField.PieceId = null;

            return BoardData.Create(PlayerId, new[] {new Piece(piece.Id, PieceType.Unknown, piece.PlayerId)});
        }
    }
}