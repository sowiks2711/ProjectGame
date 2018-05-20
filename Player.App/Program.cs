﻿using System;
using System.Collections.Generic;
using System.Net;
using BoardGenerators.Loaders;
using Common;
using Communication.Client;
using GameMaster.Configuration;
using Messaging.ErrorsMessages;
using Messaging.Serialization;
using Mono.Options;
using NLog;

namespace Player.App
{
    internal class Program
    {
        private static VerboseLogger _logger;

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var player = CreatePlayerFrom(args);

            _logger = player.VerboseLogger;
        }

        private static Player CreatePlayerFrom(IEnumerable<string> parameters)
        {
            bool teamFlag = false, roleFlag = false, addressFlag = false;
            var ipAddress = default(IPAddress);
            var port = default(int);
            var gameConfigPath = default(string);
            var gameName = default(string);
            var team = default(TeamColor);
            var role = default(PlayerType);
            var loggingMode = LoggingMode.NonVerbose;

            var options = new OptionSet
            {
                {"port=", "port number", (int p) => port = p},
                {"conf=", "configuration filename", c => gameConfigPath = c},
                {"address=", "server adress or hostname", a => addressFlag = IPAddress.TryParse(a, out ipAddress)},
                {"game=", "name of the game", g => gameName = g},
                {"team=", "red|blue", t => teamFlag = Enum.TryParse(t, true, out team)},
                {"role=", "leader|player", r => roleFlag = Enum.TryParse(r, true, out role)},
                {"verbose:", "logging mode", v => loggingMode = LoggingMode.Verbose }
            };

            options.Parse(parameters);

            if (!addressFlag)
            {
                addressFlag = true;
                var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                ipAddress = ipHostInfo.AddressList[0];
            }

            if (port == default(int) || gameConfigPath == default(string) || gameName == default(string) ||
                !addressFlag || !teamFlag || !roleFlag)
                Usage(options);


            var configLoader = new XmlLoader<GameConfiguration>();
            var config = configLoader.LoadConfigurationFromFile(gameConfigPath);

            var keepAliveInterval = TimeSpan.FromMilliseconds((int) config.KeepAliveInterval);
            var communicationClient = new AsynchronousCommunicationClient(new IPEndPoint(ipAddress, port), keepAliveInterval,
                MessageSerializer.Instance);

            var player = new Player(communicationClient, gameName, team, role, new ErrorsMessagesFactory(), loggingMode);

            return player;
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