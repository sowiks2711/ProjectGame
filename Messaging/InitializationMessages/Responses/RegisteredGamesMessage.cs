﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Common;
using Common.Interfaces;

namespace Messaging.InitializationMessages
{
    /// <summary>
    ///     CS's response to player about listing all joinable games
    /// </summary>
    [XmlType(XmlRootName)]
    public class RegisteredGamesMessage : Message
    {
        public const string XmlRootName = "RegisteredGames";

        protected RegisteredGamesMessage()
        {
        }

        public RegisteredGamesMessage(IEnumerable<GameInfo> games)
        {
            Games = games.ToArray();
        }

        [XmlElement("GameInfo")]
        public GameInfo[] Games { get; set; }

        public override IMessage Process(IGameMaster gameMaster)
        {
            throw new NotImplementedException();
        }

        public override void Process(IPlayer player)
        {
            player.UpdateGameState(Games);
        }

        public override void Process(ICommunicationServer cs, int id)
        {
            throw new NotImplementedException();
        }

        public override string ToLog()
        {
            return string.Join(',', XmlRootName);
        }
    }
}