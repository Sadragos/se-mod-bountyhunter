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
        public List<Death> KillList = new List<Death>();

        [ProtoMember(7)]
        public List<Death> DeathList = new List<Death>();

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

        internal void RemoveClaimable(Item item, float amount)
        {
            foreach(Item bounty in ClaimableBounty)
            {
                if(bounty.ItemId.Equals(item.ItemId))
                {
                    bounty.Value -= amount;
                    if (bounty.Value <= 0) ClaimableBounty.Remove(bounty);
                    break;
                }
            }
        }

        internal void AddDeath(string reason, string killer, float claimedBounty = 0)
        {
            Deaths++;
            while (DeathList.Count >= Config.Instance.DeathListEntries)
            {
                DeathList.RemoveAt(DeathList.Count - 1);
            }
            DeathList.Insert(0, new Death(killer, Utils.Utilities.CurrentTimestamp(), reason, claimedBounty));
        }

        internal void AddKill(string reason, string victim, float claimedBounty = 0)
        {
            Kills++;
            while (KillList.Count >= Config.Instance.DeathListEntries)
            {
                KillList.RemoveAt(KillList.Count - 1);
            }
            KillList.Insert(0, new Death(victim, Utils.Utilities.CurrentTimestamp(), reason, claimedBounty));
        }
    }
}