﻿using PlayerStateCoordinator.Common;
using PlayerStateCoordinator.Common.Transitions;
using PlayerStateCoordinator.GamePlay.NormalPlayer.Transitions;

namespace PlayerStateCoordinator.GamePlay.NormalPlayer.States
{
    public class InGoalAreaMovingToTaskStrategyState : NormalPlayerStrategyState
    {
        public InGoalAreaMovingToTaskStrategyState(NormalPlayerStrategyInfo playerStrategyInfo) : base(
            StateTransitionType.Triggered,
            playerStrategyInfo)
        {
            Transitions = new Transition[]
            {
                new IsNormalPlayerBlockedTransition(playerStrategyInfo, this),
                new IsInGoalWithoutPieceStrategyTransition(playerStrategyInfo),
                new IsInTaskAreaStrategyTransition(playerStrategyInfo)
            };
        }
    }
}