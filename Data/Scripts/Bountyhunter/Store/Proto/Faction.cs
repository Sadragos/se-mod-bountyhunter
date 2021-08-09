using System;
using ProtoBuf;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class Faction : Participant<long>
    {
        [ProtoMember(14)]
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
    }
}