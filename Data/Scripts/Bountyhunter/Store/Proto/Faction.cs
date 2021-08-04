using System;
using ProtoBuf;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class Faction
    {

        [ProtoMember(1)]
        [XmlAttribute]
        public string Name;

        [ProtoMember(2)]
        [XmlAttribute]
        public string Tag;

        [ProtoMember(3)]
        [XmlAttribute]
        public long Id;

        [ProtoMember(4)]
        public List<string> Members = new List<string>();

        [ProtoMember(5)]
        public List<Bounty> Bounties = new List<Bounty>();
    }
}