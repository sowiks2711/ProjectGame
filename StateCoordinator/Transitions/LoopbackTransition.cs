﻿using System.Collections.Generic;
using Common.Interfaces;
using PlayerStateCoordinator.States;

namespace PlayerStateCoordinator.Transitions
{
    public class LoopbackTransition : Transition
    {
        public LoopbackTransition(State nextState, IEnumerable<IMessage> message)
        {
            NextState = nextState;
            Message = message;
        }

        public override State NextState { get; }

        public override IEnumerable<IMessage> Message { get; }

        public override bool IsPossible()
        {
            return true;
        }
    }
}