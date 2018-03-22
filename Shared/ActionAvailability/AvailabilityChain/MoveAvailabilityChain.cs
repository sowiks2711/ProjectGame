﻿using Shared.ActionAvailability.AvailabilityLink;
using Shared.BoardObjects;
using static Shared.CommonResources;

namespace Shared.ActionAvailability.AvailabilityChain
{
    public class MoveAvailabilityChain : IAvailabilityChain
    {
        private readonly Board board;
        private readonly MoveType direction;
        private readonly Location location;
        private readonly TeamColour team;

        public MoveAvailabilityChain(Location location, MoveType direction, TeamColour team, Board board)
        {
            this.location = location;
            this.direction = direction;
            this.team = team;
            this.board = board;
        }

        public bool ActionAvailable()
        {
            var builder =
                new AvailabilityChainBuilder(new IsInsideBoardLink(location, direction, board.Width, board.Height))
                    .AddNextLink(new IsAvailableTeamAreaLink(location, direction, board.GoalAreaSize,
                        board.TaskAreaSize, team))
                    .AddNextLink(new IsFieldPlayerUnoccupiedLink(location, direction, board));
            return builder.Build().ValidateLink();
        }
    }
}