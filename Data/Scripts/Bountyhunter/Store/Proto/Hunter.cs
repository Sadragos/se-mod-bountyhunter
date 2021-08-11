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
    }
}