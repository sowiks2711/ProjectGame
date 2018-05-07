﻿namespace Common.Interfaces
{
    public interface IAsynchronousSocketListener
    {
        void Send(IMessage message, int socketId);
        void StartListening();
    }
}