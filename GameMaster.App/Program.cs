﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using BoardGenerators.Loaders;
using Common;
using Communication.Client;
using GameMaster.Configuration;
using Messaging.Serialization;
using Mono.Options;
using NLog;

namespace GameMaster.App
{
    internal class Program
    {
        private static ILogger _logger;
        private static string _finishedGameMessage = "";

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            var gm = CreateGameMasterFrom(args);
            gm.GameFinished += GenerateNewFinishedGameMessage;
            _logger = GameMaster.Logger;
            while (true)
            {
                var boardVisualizer = new BoardVisualizer();
                for (var i = 0;; i++)
                {
                    Thread.Sleep(200);
                    boardVisualizer.VisualizeBoard(gm.Board);
                    Console.WriteLine(i);
                    Console.WriteLine(_finishedGameMessage);
                }
            }
        }

        private static void GenerateNewFinishedGameMessage(object sender, GameFinishedEventArgs e)
        {
            _finishedGameMessage = "Last game winners: " + (e.Winners == TeamColor.Red ? "Red " : "Blue");
        }

        private static GameMaster CreateGameMasterFrom(IEnumerable<string> parameters)
        {
            var addressFlag = false;
            var port = default(int);
            var gameConfigPath = default(string);
            var address = default(IPAddress);
            var gameName = default(string);

            var options = new OptionSet
            {
                {"port=", "port number", (int p) => port = p},
                {"conf=", "configuration filename", c => gameConfigPath = c},
                {"address=", "server adress or hostname", a => addressFlag = IPAddress.TryParse(a, out address)},
                {"game=", "name of the game", g => gameName = g}
            };

            options.Parse(parameters);

            if (port == default(int) || gameConfigPath == default(string) || gameName == default(string) ||
                !addressFlag)
                Usage(options);

            var configLoader = new XmlLoader<GameConfiguration>();
            var config = configLoader.LoadConfigurationFromFile(gameConfigPath);

            var communicationClient = new AsynchronousClient(new TcpSocketConnector(MessageSerializer.Instance, port,
                address,
                TimeSpan.FromMilliseconds((int) config.KeepAliveInterval)));

            return new GameMaster(config, communicationClient, gameName);
        }

        private static void Usage(OptionSet options)
        {
            Console.WriteLine("Usage:");
            options.WriteOptionDescriptions(Console.Out);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            ApplicationFatalException.HandleFatalException(args, _logger);
        }
    }
}