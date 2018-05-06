﻿using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Common.Interfaces;

namespace Communication
{
    public enum ClientType
    {
        NonInitialized,
        GameMaster,
        Player
    };

    public abstract class TcpConnection : ITcpConnection
    {
        private readonly IMessageDeserializer _messageDeserializer;

        protected TcpConnection(Socket workSocket, int id, IMessageDeserializer messageDeserializer)
        {
            WorkSocket = workSocket;
            Id = id;
            _messageDeserializer = messageDeserializer;
            MessageProcessed = new ManualResetEvent(true);
            State = new CommunicationState();
            ClientType = ClientType.NonInitialized;
        }

        private int Id { get; set; }
        private Socket WorkSocket { get; set; }

        private ManualResetEvent MessageProcessed { get; }
        private CommunicationState State { get; set; }

        public int SocketId => Id;
        private ClientType ClientType { get; set; }

        public void Receive()
        {
            MessageProcessed.Reset();

            try
            {
                WorkSocket.BeginReceive(State.Buffer, 0, CommunicationState.BufferSize, 0,
                    ReadCallback, State);
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }

                throw;
            }

            MessageProcessed.WaitOne();
        }

        public virtual void Send(byte[] byteData)
        {
            try
            {
                WorkSocket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, WorkSocket);
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }

                throw;
            }
        }

        public void SendKeepAlive()
        {
            Send(new[] {Convert.ToByte(CommunicationState.EtbByte)});
        }

        public void CloseSocket()
        {
            if (WorkSocket == null)
                return;

            try
            {
                WorkSocket.Shutdown(SocketShutdown.Both);
                WorkSocket.Close();
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }

                throw;
            }
        }

        public void FinalizeConnect(IAsyncResult ar)
        {
            WorkSocket.EndConnect(ar);
            Debug.WriteLine($"Socket connected to {WorkSocket.RemoteEndPoint}");
        }

        public long GetLastMessageReceivedTicks()
        {
            return State.LastMessageReceivedTicks;
        }

        public void MarkClientAsPlayer()
        {
            ClientType = ClientType.Player;
        }

        public void MarkClientAsGameMaster()
        {
            ClientType = ClientType.GameMaster;
        }

        public void UpdateLastMessageTicks()
        {
            State.UpdateLastMessageTicks();
        }

        public abstract void Handle(IMessage message, int id = -404);
        public abstract void HandleKeepAliveMessage();

        private void ReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as CommunicationState;
            var handler = WorkSocket;
            var bytesRead = 0;

            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }

                throw;
            }

            if (bytesRead > 0)
            {
                var (messages, hasEtbByte) = state.SplitMessages(bytesRead, Id);

                var handledKeepAlive = false;
                foreach (var message in messages)
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        Handle(_messageDeserializer.Deserialize(message), Id);
                    }
                    HandleKeepAliveMessage();
                }

                //DONT TOUCH THAT 
                //DANGER ZONE ************
                if (!hasEtbByte)
                {
                    try
                    {
                        handler.BeginReceive(state.Buffer, 0, CommunicationState.BufferSize, 0,
                            ReadCallback, state);
                    }
                    catch (Exception e)
                    {
                        if (e is SocketException || e is ObjectDisposedException)
                        {
                            Console.WriteLine(e.ToString());
                            return;
                        }

                        throw;
                    }
                }
                else
                {
                    MessageProcessed.Set();
                }
                // ManualResetEvent (Semaphore) is signaled only when whole message was received,
                // allowing another thread to start evaluating ReadCallback. Otherwise the same thread 
                // continues to read the rest of the message
                //DANGER ZONE ****************
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                var handler = (Socket) ar.AsyncState;
                var bytesSent = handler.EndSend(ar);
                Debug.WriteLine($"Sent {bytesSent} bytes to client.");
            }
            catch (Exception e)
            {
                if (e is SocketException || e is ObjectDisposedException)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }

                throw;
            }
        }
    }
}