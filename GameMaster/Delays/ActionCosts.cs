﻿using System;
using System.Collections.Generic;
using Common.ActionInfo;

namespace GameMaster.Delays
{
    public class ActionCosts : IEquatable<ActionCosts>
    {
        public double MoveDelay { get; set; }
        public double DiscoverDelay { get; set; }
        public double TestDelay { get; set; }
        public double PickUpDelay { get; set; }
        public double PlacingDelay { get; set; }
        public double KnowledgeExchangeDelay { get; set; }
        public double DestroyDelay { get; set; }

        public bool Equals(ActionCosts other)
        {
            return other != null &&
                   MoveDelay == other.MoveDelay &&
                   DiscoverDelay == other.DiscoverDelay &&
                   TestDelay == other.TestDelay &&
                   PickUpDelay == other.PickUpDelay &&
                   PlacingDelay == other.PlacingDelay &&
                   KnowledgeExchangeDelay == other.KnowledgeExchangeDelay;
        }

        internal double GetDelayFor(ActionInfo actionInfo)
        {
            return GetDelayFor((dynamic) actionInfo);
        }

        private double GetDelayFor(MoveActionInfo actionInfo)
        {
            return MoveDelay;
        }

        private double GetDelayFor(DiscoverActionInfo actionInfo)
        {
            return DiscoverDelay;
        }

        private double GetDelayFor(PickUpActionInfo actionInfo)
        {
            return PickUpDelay;
        }

        private double GetDelayFor(PlaceActionInfo actionInfo)
        {
            return PlacingDelay;
        }

        private double GetDelayFor(TestActionInfo actionInfo)
        {
            return TestDelay;
        }

        private double GetDelayFor(DestroyActionInfo actionInfo)
        {
            return DestroyDelay;
        }

        private double GetDelayFor(KnowledgeExchangeInfo actionInfo)
        {
            return KnowledgeExchangeDelay;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ActionCosts);
        }

        public override int GetHashCode()
        {
            var hashCode = -582915105;
            hashCode = hashCode * -1521134295 + MoveDelay.GetHashCode();
            hashCode = hashCode * -1521134295 + DiscoverDelay.GetHashCode();
            hashCode = hashCode * -1521134295 + TestDelay.GetHashCode();
            hashCode = hashCode * -1521134295 + PickUpDelay.GetHashCode();
            hashCode = hashCode * -1521134295 + PlacingDelay.GetHashCode();
            hashCode = hashCode * -1521134295 + KnowledgeExchangeDelay.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ActionCosts costs1, ActionCosts costs2)
        {
            return EqualityComparer<ActionCosts>.Default.Equals(costs1, costs2);
        }

        public static bool operator !=(ActionCosts costs1, ActionCosts costs2)
        {
            return !(costs1 == costs2);
        }
    }
}