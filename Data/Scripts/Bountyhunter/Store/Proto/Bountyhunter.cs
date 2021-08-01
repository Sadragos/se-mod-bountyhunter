using System;
using ProtoBuf;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class Bountyhunter
    {

        [ProtoMember(1)]
        [XmlAttribute]
        public string Name;

        [ProtoMember(2)]
        [XmlAttribute]
        public string Id;

        [ProtoMember(3)]
        [XmlAttribute]
        public string FactionTag;

        [ProtoMember(4)]
        [XmlAttribute]
        public int Kills;

        [ProtoMember(5)]
        [XmlAttribute]
        public int Deaths;

    }
}