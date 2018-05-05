﻿using FluentAssertions;
using Messaging.Serialization;
using TestScenarios;
using Xunit;

namespace Player.Tests
{
    public class ActionsTests
    {
        [Theory]
        [ClassData (typeof(TestsDataset))]
        public void TestActionBoardsUpdate(ScenarioBase scenario)
        {
            var player = new Player(MessageSerializer.Instance);
            var playerInfo = scenario.InitialPlayerBoard.Players[scenario.PlayerId];
            player.InitializePlayerWithoutCommunicationClient(scenario.PlayerId, scenario.PlayerGuid, playerInfo.Team, playerInfo.Role,
                scenario.InitialPlayerBoard, playerInfo.Location);

            scenario.Response.Process(player);
            
            player.Board.Should().BeEquivalentTo(scenario.UpdatedPlayerBoard);
            player.Board.Should().Be(scenario.UpdatedPlayerBoard);
        }
    }
}