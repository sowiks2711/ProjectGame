﻿using PlayerStateCoordinator.Info;
using PlayerStateCoordinator.Transitions;
using PlayerStateCoordinator.Transitions.GameStrategyTransitions;

namespace PlayerStateCoordinator.States.GameStrategyStates
{
    public class DiscoverStrategyState : NormalPlayerStrategyState
    {
        public DiscoverStrategyState(GameStrategyInfo gameStrategyInfo) : base(
            StateTransitionType.Triggered,
            gameStrategyInfo)
        {
            Transitions = new Transition[]
            {
                new MoveToPieceStrategyTransition(gameStrategyInfo)
            };
        }
    }
}