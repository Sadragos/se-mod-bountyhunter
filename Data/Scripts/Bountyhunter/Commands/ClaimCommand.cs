using Bountyhunter.Store;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRageMath;

namespace Bountyhunter.Commands
{
    class ClaimCommand : AbstactCommandHandler
    {
        public ClaimCommand() : base("claim") {

        }

        public override string ArgumentDescription()
        {
            return "";
        }

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            Hunter hunter = Participants.GetPlayer(player);
            List<Item> rewards = new List<Item>();

            bool creditsPaid = false;
            if (!Config.Instance.CreditsAsItem)
            {
                creditsPaid = PayCredits(player, hunter);
            }

            if (Config.Instance.SpawnRewardDropPods)
            {
                PayoutLootbox(player, hunter);
            } else
            {
                PayoutInventory(player, hunter, creditsPaid);
            }
        }

        private void PayoutLootbox(IMyPlayer player, Hunter hunter)
        {
            List<Item> payout = GetPayout(hunter);
            if (payout.Count == 0) return;

            List<Item> ToAdd = new List<Item>();
            float remainingVolume = LootboxSpawner.MaxBoxSize;
            bool full = false;
            foreach(Item item in payout)
            {
                float volume = Utilities.ItemVolume(item.ItemId);
                float maxAmount = remainingVolume / volume;
                float newValue = Math.Min(item.Value, maxAmount);
                if (!item.HasFractions) newValue = (float)Math.Floor(newValue);
                if(newValue <= maxAmount)
                {
                    full = true;
                }
                item.Value = newValue;
                ToAdd.Add(item);
                hunter.RemoveClaimable(item, newValue);
                if(full)
                {
                    break;
                }
            }
            LootboxSpawner.SpawnLootBox(player, ToAdd);
        }

        private void PayoutInventory(IMyPlayer player, Hunter hunter, bool creditsPaid)
        {
            List<Item> payout = GetPayout(hunter);
            if (payout.Count == 0 && !creditsPaid)
            {
                Utilities.ShowChatMessage("You dont have enough Bounty collected.", player.IdentityId);
            }
            if (payout.Count == 0) return;

            List<NamedInventory> inventories = Utilities.GetInventoriesNearPlayer(player);
            if(inventories.Count == 0)
            {
                Utilities.ShowChatMessage("Could't find Inventory to deposit Items.", player.IdentityId);
                return;
            }

            int currentInventoryIndex = 0;
            NamedInventory currentInventory = inventories[currentInventoryIndex];

            StringBuilder builder = new StringBuilder("Items paid out:");

            foreach(Item item in payout)
            {
                while(item.Value >= 0.001 && currentInventoryIndex < inventories.Count)
                {
                    float amount = Utilities.TryPutItem(currentInventory.Inventory, item.ItemId, item.Value);
                    if (amount > 0)
                    {
                        builder.Append("\n -> ");
                        builder.Append(Formater.FormatNumber(amount));
                        builder.Append(" ");
                        builder.Append(Values.Items[item.ItemId].ToString());
                        builder.Append(" into ");
                        builder.Append(currentInventory.Name);
                    }

                    if (amount < item.Value-Config.Instance.FloatAmountBuffer)
                    {
                        currentInventoryIndex++;
                        if(currentInventoryIndex >= inventories.Count)
                        {
                            builder.Append("\nthere is more, that could not fit into any Inventory near you.");
                        } else
                        {
                            currentInventory = inventories[currentInventoryIndex];
                        }
                    }

                    if (amount > 0)
                    {
                        item.Value -= amount;
                        hunter.RemoveClaimable(item, amount);
                    }
                }
            }
            Utilities.ShowChatMessage(builder.ToString(), player.IdentityId);
        }

        private List<Item> GetPayout(Hunter hunter)
        {
            List<Item> payout = new List<Item>();
            foreach(Item item in hunter.ClaimableBounty)
            {
                if (!Config.Instance.CreditsAsItem && item.ItemId.Equals(Values.SC_ITEM)) continue;
                if (item.Value < 1 && !item.HasFractions) continue;
                payout.Add(new Item(item.ItemId, (float)(item.HasFractions ? item.Value : Math.Floor(item.Value))));
            }
            return payout;
        }

        private bool PayCredits(IMyPlayer player, Hunter hunter)
        {
            foreach (Item item in hunter.ClaimableBounty)
            {
                if (item.ItemId.Equals(Values.SC_ITEM))
                {
                    long amount = (long)Math.Floor(item.Value);
                    if (amount > 1)
                    {
                        Utilities.PayPlayer(player, amount);
                        hunter.RemoveClaimable(item, amount);
                        Utilities.ShowChatMessage(Formater.FormatNumber(amount) + " Credits paid to your Account.");
                        return true;
                    }
                    break;
                }
            }
            return false;
        }
    }
}
