﻿using System.Collections.Generic;
using Common.ActionAvailability.Helpers;
using Common.BoardObjects;
using Xunit;

namespace Common.Tests.ActionAvailability
{
    public class PickUpAvailabilityTests
    {
        public PickUpAvailabilityTests()
        {
            board = new MockBoard(boardWidth, taskAreaSize, goalAreaSize)
            {
                [new Location(1, 3)] = {PlayerId = 1},
                [new Location(3, 3)] = {PlayerId = 2},
                [new Location(2, 4)] = {PlayerId = 3},
                [new Location(2, 2)] = {PlayerId = 4}
            };
            locationFail = new Location(2, 3);
            locationSuccess = new Location(1, 3);
            goalAreaLocation = new Location(1, 1);

            board.PlacePieceInTaskArea(1, locationSuccess);

            playerGuidToPieceId = new Dictionary<string, int>();
            playerGuidToPieceId.Add(playerGuidFailPickUp, pieceId);
        }

        private readonly int boardWidth = 5;
        private readonly int goalAreaSize = 2;
        private readonly int taskAreaSize = 4;
        private readonly int pieceId = 1;
        private readonly string playerGuidSuccessPickUp = "c094cab7-da7b-457f-89e5-a5c51756035f";
        private readonly string playerGuidFailPickUp = "c094cab7-da7b-457f-89e5-a5c51756035d";
        private readonly Dictionary<string, int> playerGuidToPieceId;
        private readonly Location locationFail;
        private readonly Location locationSuccess;
        private readonly Location goalAreaLocation;
        private readonly MockBoard board;

        [Fact]
        public void PickUpOnEmptyTaskField()
        {
            Assert.False(new PieceRelatedAvailability().IsPieceInCurrentLocation(locationFail, board));
        }

        [Fact]
        public void PickUpOnGoalArea()
        {
            Assert.False(new PieceRelatedAvailability().IsPieceInCurrentLocation(goalAreaLocation, board));
        }

        [Fact]
        public void PickUpOnTaskFieldWithPiece()
        {
            Assert.True(new PieceRelatedAvailability().IsPieceInCurrentLocation(locationSuccess, board));
        }

        [Fact]
        public void PickUpWhenPlayerCarringPiece()
        {
            Assert.False(
                new PieceRelatedAvailability().HasPlayerEmptySlotForPiece(playerGuidFailPickUp, playerGuidToPieceId));
        }

        [Fact]
        public void PickUpWhenPlayerNotCarringPiece()
        {
            Assert.True(
                new PieceRelatedAvailability().HasPlayerEmptySlotForPiece(playerGuidSuccessPickUp,
                    playerGuidToPieceId));
        }
    }
}