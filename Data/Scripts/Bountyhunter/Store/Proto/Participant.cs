using System;
using ProtoBuf;
using System.Xml.Serialization;
using System.Collections.Generic;
using Bountyhunter.Utils;


namespace Bountyhunter.Store.Proto
{
    public class Participant<IDType>
    {
        [ProtoMember(1)]
        [XmlAttribute]
        public string Name;

        [ProtoMember(2)]
        [XmlAttribute]
        public IDType Id;

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
        public List<Death> KillList = new List<Death>();

        [ProtoMember(7)]
        public List<Death> DeathList = new List<Death>();

        [ProtoMember(8)]
        public List<Bounty> Bounties = new List<Bounty>();

        [ProtoMember(9)]
        [XmlAttribute]
        public double BountyPlaced = 0;

        [ProtoMember(10)]
        [XmlAttribute]
        public double BountyClaimed = 0;

        [ProtoMember(11)]
        [XmlAttribute]
        public double BountyReceived = 0;


        [ProtoMember(12)]
        [XmlAttribute]
        public double DamageDone = 0;

        [ProtoMember(13)]
        [XmlAttribute]
        public double DamageReceived = 0;

        internal void AddDeath(string reason, string killer, float claimedBounty = 0)
        {
            Deaths++;
            while (DeathList.Count >= Config.Instance.DeathListEntries)
            {
                DeathList.RemoveAt(DeathList.Count - 1);
            }
            DeathList.Insert(0, new Death(killer, Utilities.CurrentTimestamp(), reason, claimedBounty));
        }

        internal void AddKill(string reason, string victim, float claimedBounty = 0)
        {
            Kills++;
            while (KillList.Count >= Config.Instance.DeathListEntries)
            {
                KillList.RemoveAt(KillList.Count - 1);
            }
            KillList.Insert(0, new Death(victim, Utilities.CurrentTimestamp(), reason, claimedBounty));
            BountyClaimed += claimedBounty;
        }

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
                    float amount = Math.Min(percent * b.RewardItem.Value, b.RewardItem.Value - b.RewardItem.Claimed);
                    b.RewardItem.Claimed += amount;
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
    }
}
