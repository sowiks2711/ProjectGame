﻿using System.Threading;
using Common.Interfaces;
using Messaging.InitialisationMessages;
using Player.Strategy.StateInfo;
using Player.Strategy.States;
using Player.Strategy.States.GameStates;

namespace Player.Strategy.Conditions.GameConditions
{
    class IsJoinUnsuccessfulCondition : GameCondition
    {
        public IsJoinUnsuccessfulCondition(GameStateInfo gameStateInfo) : base(gameStateInfo)
        {
        }

        public override bool CheckCondition()
        {
            return !GameStateInfo.JoiningSuccessful;
        }

        public override BaseState GetNextState(BaseState fromStrategyState)
        {
            return new MatchingGameState(GameStateInfo);
        }

        public override IMessage GetNextMessage(BaseState fromStrategyState)
        {
            Thread.Sleep(1000);
            return new GetGamesMessage();
        }

        public override bool ReturnsMessage(BaseState fromStrategyState)
        {
            return true;
        }
    }
}
