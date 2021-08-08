using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class Death
    {
        [ProtoMember(1)]
        [XmlAttribute]
        public string opponent;
        [ProtoMember(2)]
        [XmlAttribute]
        public string time;
        [ProtoMember(3)]
        [XmlAttribute]
        public string reason;
        [ProtoMember(4)]
        [XmlAttribute]
        public float claimedBounty;

        public Death(string opponent, string time, string reason, float claimedBounty)
        {
            this.opponent = opponent;
            this.time = time;
            this.reason = reason;
            this.claimedBounty = claimedBounty;
        }

        public Death()
        {
        }
    }
}
