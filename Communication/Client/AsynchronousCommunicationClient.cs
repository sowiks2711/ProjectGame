﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common;
using Common.Interfaces;
using Communication.Exceptions;

namespace Communication.Client
{
    public class AsynchronousCommunicationClient : ICommunicationClient
    {
        private readonly ManualResetEvent _connectDone;
        private readonly ManualResetEvent _connectFinalized;
        private readonly IPEndPoint _ipEndPoint;
        private readonly TimeSpan _keepAliveInterval;
        private readonly IMessageDeserializer _messageDeserializer;

        private ITcpConnection _tcpConnection;

        public AsynchronousCommunicationClient(IPEndPoint endPoint, TimeSpan keepAliveInterval,
            IMessageDeserializer messageDeserializer)

        {
            _connectFinalized = new ManualResetEvent(false);
            _connectDone = new ManualResetEvent(false);
            _messageDeserializer = messageDeserializer;
            _ipEndPoint = endPoint;
            _keepAliveInterval = keepAliveInterval == default(TimeSpan)
                ? Constants.DefaultMaxUnresponsivenessDuration
                : keepAliveInterval;
        }

        public void Send(IMessage message)
        {
            _connectFinalized.WaitOne();
            var byteData = Encoding.ASCII.GetBytes(message.SerializeToXml() + Constants.EtbByte);
            try
            {
                _tcpConnection.Send(byteData);
            }
            catch (Exception e)
            {
                ConnectionException.PrintUnexpectedConnectionErrorDetails(e);
                throw;
            }
        }

        public void Connect(Action<CommunicationException> connectionFailureHandler, Action<IMessage> messageHandler)
        {
            try
            {
                var client = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _tcpConnection = new ClientTcpConnection(client, -1, connectionFailureHandler, _messageDeserializer,
                    messageHandler);

                client.BeginConnect(_ipEndPoint, ConnectCallback, client);
                _connectDone.WaitOne();
            }
            catch (Exception e)
            {
                ConnectionException.PrintUnexpectedConnectionErrorDetails(e);
                throw;
            }

            StartReading();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _tcpConnection.FinalizeConnect(ar);
                _connectDone.Set();
                _connectFinalized.Set();
            }
            catch (Exception e)
            {
                ConnectionException.PrintUnexpectedConnectionErrorDetails(e);
                throw;
            }
        }

        private void StartReading()
        {
            while (true)
                try
                {
                    _tcpConnection.Receive();
                }
                catch (Exception e)
                {
                    ConnectionException.PrintUnexpectedConnectionErrorDetails(e);
                    throw;
                }
        }
    }
}