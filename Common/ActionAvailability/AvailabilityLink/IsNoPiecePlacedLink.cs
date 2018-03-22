﻿using Common.ActionAvailability.ActionAvailabilityHelpers;
using Common.BoardObjects;
using Common.Interfaces;

namespace Common.ActionAvailability.AvailabilityLink
{
    internal class IsNoPiecePlacedLink : AvailabilityLinkBase
    {
        private readonly IBoard board;
        private readonly Location location;

        public IsNoPiecePlacedLink(Location location, IBoard board)
        {
            this.location = location;
            this.board = board;
        }

        protected override bool Validate()
        {
            return !new PieceRelatedAvailability().IsPieceInCurrentLocation(location, board);
        }
    }
}