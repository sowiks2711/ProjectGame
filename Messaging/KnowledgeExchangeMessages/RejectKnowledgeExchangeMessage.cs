﻿using System;
using System.Xml.Serialization;
using Common.Interfaces;

namespace Messaging.KnowledgeExchangeMessages
{
    [XmlType(XmlRootName)]
    public class RejectKnowledgeExchangeMessage : BetweenPlayersMessage
    {
        public const string XmlRootName = "RejectKnowledgeExchange";

        protected RejectKnowledgeExchangeMessage()
        {
        }

        public RejectKnowledgeExchangeMessage(int senderId, int withPlayerId, bool permanent = false) : base(senderId,
            withPlayerId)
        {
            Permanent = permanent;
        }

        public bool Permanent { get; set; }

        public override IMessage Process(IGameMaster gameMaster)
        {
            Console.WriteLine($"Player #{SenderPlayerId} rejected");
            gameMaster.KnowledgeExchangeManager.HandleExchangeRejection(SenderPlayerId, PlayerId);
            return this;
        }

        public override void Process(IPlayer player)
        {
            Console.WriteLine($"Player #{SenderPlayerId} rejected communication");
        }

        public override void Process(ICommunicationServer cs, int id)
        {
            var gameId = cs.GetGameIdFor(id);
            cs.Send(this, gameId == id ? PlayerId : gameId);
        }

        public override string ToLog()
        {
            return XmlRootName + base.ToLog();
        }
    }
}