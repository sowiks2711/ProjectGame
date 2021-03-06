﻿using System;
using System.Xml.Serialization;
using Common;
using Common.Interfaces;

namespace Messaging.InitializationMessages
{
    /// <summary>
    ///     GM sends confirmation to player about his registration
    /// </summary>
    [XmlType(XmlRootName)]
    public class ConfirmJoiningGameMessage : MessageToPlayer
    {
        public const string XmlRootName = "ConfirmJoiningGame";

        protected ConfirmJoiningGameMessage()
        {
        }

        public ConfirmJoiningGameMessage(int gameId, int playerId, Guid privateGuid, PlayerBase playerDefiniton)
        {
            GameId = gameId;
            PlayerId = playerId;
            PrivateGuid = privateGuid;
            PlayerDefinition = new PlayerBase(playerDefiniton.Id, playerDefiniton.Team, playerDefiniton.Role);
        }

        [XmlAttribute("gameId")] public int GameId { get; set; }
        [XmlAttribute("privateGuid")] public Guid PrivateGuid { get; set; }
        public PlayerBase PlayerDefinition { get; set; }

        public override IMessage Process(IGameMaster gameMaster)
        {
            throw new NotImplementedException();
        }

        public override void Process(IPlayer player)
        {
            player.UpdateJoiningInfo(true);
            player.UpdatePlayer(PlayerId, PrivateGuid, PlayerDefinition, GameId);
        }

        public override void Process(ICommunicationServer cs, int id)
        {
            cs.UpdateTeamCount(id, PlayerDefinition.Team);
            cs.AssignGameIdToPlayerId(id, PlayerId);
            cs.Send(this, PlayerId);
        }

        public override string ToLog()
        {
            return string.Join(',', XmlRootName, base.ToLog());
        }
    }
}