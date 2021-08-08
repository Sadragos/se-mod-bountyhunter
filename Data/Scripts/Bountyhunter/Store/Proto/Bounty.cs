using System;
using ProtoBuf;
using System.Xml.Serialization;
using System.Collections.Generic;
using Bountyhunter.Utils;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class Bounty
    {

        [ProtoMember(1)]
        [XmlAttribute]
        public string Client;

        [ProtoMember(2)]
        [XmlAttribute]
        public string Timestamp = Utilities.CurrentTimestamp();

        [ProtoMember(3)]
        [XmlAttribute]
        public bool HideClient = false;

        [ProtoMember(4)]
        [XmlAttribute]
        public EBountyType BountyType;

        [ProtoMember(5)]
        [XmlAttribute]
        public float Count = 1;

        [ProtoMember(6)]
        [XmlAttribute]
        public bool Partial = true;

        [ProtoMember(7)]
        public BountyItem RewardItem = new BountyItem(Values.SC_ITEM);


        public void RecalculateWorth()
        {
            RewardItem.RecalculateWorth();
        }
    }

    public enum EBountyType
    {
        Kill,
        Damage
    }

    public enum ETargetType
    {
        Player,
        Faction
    }
}