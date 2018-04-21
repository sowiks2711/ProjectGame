﻿using Common.Interfaces;

namespace Messaging
{
    public abstract class Message : IMessage
    {
        public abstract IMessage Process(IGameMaster gameMaster);
        public abstract void Process(IGameMaster gameMaster, int i);
        public abstract bool Process(IPlayer player);
        public abstract void Process(ICommunicationServer cs, int id);

        
    }
}