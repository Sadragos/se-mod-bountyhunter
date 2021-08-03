using System;
using ProtoBuf;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class Hunter
    {

        [ProtoMember(1)]
        [XmlAttribute]
        public string Name;

        [ProtoMember(2)]
        [XmlAttribute]
        public ulong Id;

        [ProtoMember(3)]
        [XmlAttribute]
        public string FactionTag;

        [ProtoMember(4)]
        [XmlAttribute]
        public int Kills = 0;

        [ProtoMember(5)]
        [XmlAttribute]
        public int Deaths = 0;

        [ProtoMember(6)]
        public List<Kill> KillList = new List<Kill>();

        [ProtoMember(7)]
        public List<Kill> DeathList = new List<Kill>();

        [ProtoMember(8)]
        public List<Item> ClaimableBounty = new List<Item>();

        [ProtoMember(9)]
        public List<Bounty> Bounties = new List<Bounty>();


        [ProtoMember(10)]
        [XmlAttribute]
        public double BountyPlaced = 0;

        [ProtoMember(11)]
        [XmlAttribute]
        public double BountyClaimed = 0;

    }
}