﻿using System;
using System.Xml.Serialization;
using Common;
using Common.ActionInfo;

namespace Messaging.Requests
{
    [XmlType(XmlRootName)]
    public class PickUpPieceRequest : RequestMessage
    {
        public const string XmlRootName = "PickUpPiece";

        protected PickUpPieceRequest()
        {
        }

        public PickUpPieceRequest(Guid playerGuid, int gameId) : base(playerGuid, gameId)
        {
        }

        public override ActionInfo GetActionInfo()
        {
            return new PickUpActionInfo(PlayerGuid);
        }

        public override string ToLog()
        {
            return string.Join(',', ActionType.PickUp, base.ToLog());
        }
    }
}