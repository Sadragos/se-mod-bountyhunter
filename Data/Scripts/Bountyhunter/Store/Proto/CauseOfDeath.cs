using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class CauseOfDeath
    {
        [ProtoMember(1)]
        [XmlAttribute]
        public string Reason;

        [ProtoMember(2)]
        [XmlAttribute]
        public string ShortName;

        [ProtoMember(3)]
        public List<DeathMessage> NoAttacker = new List<DeathMessage>();

        [ProtoMember(4)]
        public List<DeathMessage> AsAttacker = new List<DeathMessage>();

        [ProtoMember(5)]
        public List<DeathMessage> AsVictim = new List<DeathMessage>();
    }
}
