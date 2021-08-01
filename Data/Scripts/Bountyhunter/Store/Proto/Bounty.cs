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
        public string Target;
        [ProtoMember(3)]
        [XmlAttribute]
        public ETargetType TargetType;
        [ProtoMember(4)]
        [XmlAttribute]
        public string Timestamp;
        [ProtoMember(5)]
        [XmlAttribute]
        public bool HideClient;
        [ProtoMember(6)]
        [XmlAttribute]
        public EBountyType BountyType;
        [ProtoMember(7)]
        [XmlAttribute]
        public float Count;
        [ProtoMember(8)]
        [XmlAttribute]
        public bool Partial;
        [ProtoMember(9)]
        [XmlAttribute]
        public float ClaimedCount;
        [ProtoMember(10)]
        [XmlAttribute]
        public float RemainingCurrency;

        [ProtoMember(11)]
        public Item RewardItem;


        public Bounty()
        {
            Timestamp = Utilities.CurrentTimestamp();
        }

        public void RecalculateRemainingCurrency()
        {
            RemainingCurrency = (Count - ClaimedCount / Count) * Values.ItemValue(RewardItem.ItemId) * RewardItem.Value;
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