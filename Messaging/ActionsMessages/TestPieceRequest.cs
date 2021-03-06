﻿using System;
using System.Xml.Serialization;
using Common;
using Common.ActionInfo;

namespace Messaging.Requests
{
    [XmlType(XmlRootName)]
    public class TestPieceRequest : RequestMessage
    {
        public const string XmlRootName = "TestPiece";

        protected TestPieceRequest()
        {
        }

        public TestPieceRequest(Guid playerGuid, int gameId) : base(playerGuid, gameId)
        {
        }

        public override ActionInfo GetActionInfo()
        {
            return new TestActionInfo(PlayerGuid);
        }

        public override string ToLog()
        {
            return string.Join(',', ActionType.Test, base.ToLog());
        }
    }
}