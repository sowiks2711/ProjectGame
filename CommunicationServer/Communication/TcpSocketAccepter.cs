﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common.Communication;
using Common.Interfaces;

namespace CommunicationServer.Communication
{
    public class TcpSocketAccepter : IAccepter
    {
        public Dictionary<int, TcpCommunicationTool> AgentToCommunicationHandler { get; set; }

        private readonly ManualResetEvent _readyForAccept = new ManualResetEvent(false);
        private int _counter;
        private Action<IMessage, int> _messageHandler;

        public TcpSocketAccepter(Action<IMessage, int> messageHandler)
        {
            AgentToCommunicationHandler = new Dictionary<int, TcpCommunicationTool>();
            _counter = 0;
            _messageHandler = messageHandler;
        }

        public void StartListening()
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, 11000);

            var listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    _readyForAccept.Reset();
                    Debug.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(AcceptCallback, listener);

                    _readyForAccept.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            _readyForAccept.Set();
            var handler = default(Socket);
            var listener = (Socket)ar.AsyncState;
            try
            {
                handler = listener.EndAccept(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Debug.WriteLine("Accepted for " + _counter);
            var state = new ServerTcpCommunicationTool(handler, _counter, new CommunicationServerConverter(), _messageHandler);
            AgentToCommunicationHandler.Add(_counter++, state);
            StartReading(state);
        }
        private void StartReading(TcpCommunicationTool tool)
        {
            while (true)
                tool.Receive();
        }
    }
}
