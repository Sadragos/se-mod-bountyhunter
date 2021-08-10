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
        public string Opponent;
        [ProtoMember(2)]
        [XmlAttribute]
        public string Time;
        [ProtoMember(3)]
        [XmlAttribute]
        public string Reason;
        [ProtoMember(4)]
        [XmlAttribute]
        public float ClaimedBounty;


        public Death(string opponent, string time, string reason, float claimedBounty)
        {
            this.Opponent = opponent;
            this.Time = time;
            this.Reason = reason;
            this.ClaimedBounty = claimedBounty;
        }

        public Death()
        {
        }
    }
}
