using System;
using ProtoBuf;
using System.Xml.Serialization;
using System.Collections.Generic;
using Bountyhunter.Utils;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class Hunter : Participant
    {
        [ProtoMember(12)]
        public List<Death> KillList = new List<Death>();

        [ProtoMember(13)]
        public List<Death> DeathList = new List<Death>();

        [ProtoMember(14)]
        public List<Item> ClaimableBounty = new List<Item>();

        [ProtoMember(15)]
        public DateTime LastSeen = DateTime.Now;

        [ProtoMember(16)]
        [XmlAttribute]
        public long OnlineMinutes = 0;

        [ProtoMember(17)]
        public DateTime GraceTime;

        [ProtoMember(18)]
        public bool Graced;

        [ProtoMember(19)]
        public DateTime BannedUntil;

        [ProtoMember(20)]
        public bool Banned;

        [ProtoMember(21)]
        public string BanReason;

        [ProtoMember(22)]
        public DateTime LastBountySet;


        internal void RemoveClaimable(Item item, float amount)
        {
            foreach (Item bounty in ClaimableBounty)
            {
                if (bounty.ItemId.Equals(item.ItemId))
                {
                    bounty.Value -= amount;
                    if (bounty.Value <= 0) ClaimableBounty.Remove(bounty);
                    return;
                }
            }
        }

        internal float AddClaimable(Item item, float amount)
        {
            bool added = false;
            foreach (Item bounty in ClaimableBounty)
            {
                if (bounty.ItemId.Equals(item.ItemId))
                {
                    bounty.Value += amount;
                    added = true;
                    break;
                }
            }
            if(!added) ClaimableBounty.Add(new Item(item.ItemId, amount));

            float value = Values.ItemValue(item.ItemId) * amount;
            BountyClaimed += value;
            return value;
        }

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

        public new List<Death> GetDeathList()
        {
            return DeathList;
        }

        public new List<Death> GetKillList()
        {
            return KillList;
        }

        public override bool Relevant()
        {
            return Bounties.Count > 0 || ClaimableBounty.Count > 0 || Banned || Graced;
        }

        [ProtoIgnore]
        [XmlIgnore]
        public List<Item> Payout
        {
            get
            {
                List<Item> payout = new List<Item>();
                foreach (Item item in ClaimableBounty)
                {
                    if (item.Value < Config.Instance.MinPayout) continue;
                    if (!Config.Instance.CreditsAsItem && item.ItemId.Equals(Values.SC_ITEM)) continue;
                    if (item.Value < 1 && !item.HasFractions) continue;
                    payout.Add(new Item(item.ItemId, (float)(item.HasFractions ? item.Value : Math.Floor(item.Value))));
                }
                return payout;
            }
        }
    }
}