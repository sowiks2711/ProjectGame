﻿using System.Collections.Generic;
using Common;
using TestScenarios.MoveScenarios;
using Xunit;

namespace Player.Tests
{
    public class MoveTests
    {
        [Theory]
        [MemberData(nameof(GetData))]
        public void MoveTestsBoard(MoveScenarioBase scenario)
        {
            var player = new Player();
            var playerInfo = scenario.InitialPlayerBoard.Players[scenario.PlayerId];
            player.InitializePlayer(scenario.PlayerId,scenario.PlayerGuid, playerInfo.Team, playerInfo.Role, scenario.InitialPlayerBoard, playerInfo.Location);

            scenario.Response.Process(player);

            Assert.Equal(scenario.UpdatedPlayerBoard, player.Board);
        }


        public static IEnumerable<object[]> GetData()
        {
            yield return new object[] {new MoveToGoalField()};
            //yield return new object[] { new MoveToTaskField()};
            //yield return new object[] { new MoveToTaskFieldWithPiece()};
            //yield return new object[] { new MoveToTaskFieldWithoutPiece()};
            //yield return new object[] { new MoveToTaskFieldOccupiedByPlayerWhoCarryPiece()};
            //yield return new object[] { new MoveToTaskFieldOccupiedByPlayerWhoDoesntCarryPiece()};
            //yield return new object[] { new MoveToTaskFieldWithPieceOccupiedByPlayerWhoCarryPiece()};
            //yield return new object[] { new MoveToTaskFieldWithPieceOccupiedByPlayerWhoDoesntCarryPiece()};
        }

    }
}
