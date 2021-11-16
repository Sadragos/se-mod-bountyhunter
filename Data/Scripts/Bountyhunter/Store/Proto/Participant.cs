using System;
using ProtoBuf;
using System.Xml.Serialization;
using System.Collections.Generic;
using Bountyhunter.Utils;
using System.Linq;

namespace Bountyhunter.Store.Proto
{
    public class Participant
    {
        [ProtoMember(1)]
        [XmlAttribute]
        public long Id;

        [ProtoMember(2)]
        public string Name;

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
        public List<Bounty> Bounties = new List<Bounty>();

        [ProtoMember(7)]
        [XmlAttribute]
        public double BountyPlaced = 0;

        [ProtoMember(8)]
        [XmlAttribute]
        public double BountyClaimed = 0;

        [ProtoMember(9)]
        [XmlAttribute]
        public double BountyReceived = 0;


        [ProtoMember(10)]
        [XmlAttribute]
        public double DamageDone = 0;

        [ProtoMember(11)]
        [XmlAttribute]
        public double DamageReceived = 0;

        internal float ClaimBounty(Hunter attacker, EBountyType type, float value = 1)
        {
            float result = 0f;
            Bounty b;
            for(int i = Bounties.Count - 1; i >= 0; i--)
            {
                b = Bounties[i];
                if(b.BountyType.Equals(type))
                {
                    float percent = value / b.Count;
                    float amount = Math.Min(percent * b.RewardItem.Value, b.RewardItem.Remaining);
                    b.RewardItem.Claimed += amount;
                    b.RewardItem.RecalculateWorth();
                    result += attacker.AddClaimable(b.RewardItem, amount);
                    if(b.RewardItem.Claimed >= b.RewardItem.Value - Config.Instance.FloatAmountBuffer)
                    {
                        Bounties.RemoveAt(i);
                    }
                }
            }
            return result;
        }

        internal void CleanupBonties()
        {
            Bounties.RemoveAll(b => b.RewardItem.Claimed >= b.RewardItem.Value - Config.Instance.FloatAmountBuffer);
        }

        [ProtoIgnore]
        [XmlIgnore]
        public float KillDeathRatio
        {
            get
            {
                if (Deaths == 0) return Kills;
                if (Kills == 0) return 0;
                return (float)Kills / (float)Deaths;
            }
        }

        [ProtoIgnore]
        [XmlIgnore]
        public double DamageRatio
        {
            get
            {
                if (DamageReceived == 0) return DamageDone;
                if (DamageDone == 0) return 0;
                return (float)DamageDone / (float)DamageReceived;
            }
        }

        [ProtoIgnore]
        [XmlIgnore]
        public float BountyWorth
        {
            get
            {
                float sum = Bounties.Sum(b => b.RewardItem.Worth); ;
                if (!string.IsNullOrEmpty(FactionTag))
                {
                    Faction faction = Participants.GetFaction(FactionTag, false);
                    if(!faction.Id.Equals(Id))
                    {
                        sum += faction.BountyWorth;
                    }
                }
                return sum;
            }
        }

        public new string ToString()
        {
            if (string.IsNullOrEmpty(FactionTag)) return Name;
            return "[" + FactionTag + "] " + Name;
        }

        public List<Death> GetDeathList()
        {
            return new List<Death>();
        }

        public List<Death> GetKillList()
        {
            return new List<Death>();
        }
    }
}
