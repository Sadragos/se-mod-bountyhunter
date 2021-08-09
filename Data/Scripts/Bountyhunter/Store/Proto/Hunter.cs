using System;
using ProtoBuf;
using System.Xml.Serialization;
using System.Collections.Generic;
using Bountyhunter.Utils;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class Hunter : Participant<ulong>
    {

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
    }
}