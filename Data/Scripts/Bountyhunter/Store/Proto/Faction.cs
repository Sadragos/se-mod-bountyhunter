using System;
using ProtoBuf;
using System.Xml.Serialization;
using System.Collections.Generic;
using Bountyhunter.Utils;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class Faction : Participant
    {
        [ProtoMember(12)]
        public List<string> Members = new List<string>();

        

        [ProtoIgnore]
        [XmlIgnore]
        public List<Hunter> Hunters
        {
            get
            {
                List<Hunter> result = new List<Hunter>();
                foreach(Hunter hunter in Participants.Players.Values)
                {
                    if (hunter.FactionTag.Equals(FactionTag)) result.Add(hunter);
                }
                return result;
            }
        }

        public override bool Relevant()
        {
            return Bounties.Count > 0;
        }
    }
}