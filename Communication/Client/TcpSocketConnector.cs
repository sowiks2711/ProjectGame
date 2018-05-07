﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Common.Interfaces;
using Communication.Exceptions;

namespace Communication.Client
{
    public class TcpSocketConnector : IConnector
    {
        private readonly IPAddress _address;
        private readonly ManualResetEvent _connectDone;
        private readonly TimeSpan _keepAliveInterval;
        private readonly IMessageDeserializer _messageDeserializer;
        private readonly int _port;

        public TcpSocketConnector(IMessageDeserializer messageDeserializer, int port, IPAddress address,
            TimeSpan keepAliveInterval = default(TimeSpan))
        {
            ConnectFinalized = new ManualResetEvent(false);
            _connectDone = new ManualResetEvent(false);
            _messageDeserializer = messageDeserializer;

            _port = port;
            _address = address;
            _keepAliveInterval = keepAliveInterval == default(TimeSpan)
                ? Constants.DefaultMaxUnresponsivenessDuration
                : keepAliveInterval;
        }

        public ITcpConnection TcpConnection { get; set; }
        public ManualResetEvent ConnectFinalized { get; set; }

        public void Connect(Action<IMessage> messageHandler)
        {
            try
            {
                var ipAddress = _address;
                var remoteEndPoint = new IPEndPoint(ipAddress, _port);

                var client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                var tcpConnection = new ClientTcpConnection(client, -1, _messageDeserializer, messageHandler);
                TcpConnection = tcpConnection;

                client.BeginConnect(remoteEndPoint, ConnectCallback, client);
                _connectDone.WaitOne();
                tcpConnection.UpdateLastMessageTicks();
                tcpConnection.StartKeepAliveTimer(_keepAliveInterval);
            }
            catch (Exception e)
            {
                throw new ConnectionException("Unable to connect", e);
            }

            StartReading();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                TcpConnection.FinalizeConnect(ar);
                _connectDone.Set();
                ConnectFinalized.Set();
            }
            catch (Exception e)
            {
                throw new ConnectionException("Unable to connect", e);
            }
        }

        private void StartReading()
        {
            while (true)
                try
                {
                    TcpConnection.Receive();
                }
                catch (Exception e)
                {
                    throw new ConnectionException("Unable to read", e);
                }
        }
    }
}