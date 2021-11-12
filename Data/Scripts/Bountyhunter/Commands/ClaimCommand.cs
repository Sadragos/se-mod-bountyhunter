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

        public override string ArgumentDescription => "\nClaims all bounties, that you have earned, that have not been claimed yet. Depending on the Settings they will be put into you Bankaccount, Player-Inventory, Cargo near you (that you can access) or will be delivered with a drop-pod.";

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            Hunter hunter = Participants.GetPlayer(player.Identity);
            List<Item> rewards = new List<Item>();

            long creditsPaid = 0;
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
            List<Item> payout = hunter.Payout;
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

        private void PayoutInventory(IMyPlayer player, Hunter hunter, long creditsPaid)
        {
            List<Item> payout = hunter.Payout;
            if (payout.Count == 0 && creditsPaid == 0)
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

            StringBuilder builder = new StringBuilder();
            if(creditsPaid > 0)
            {
                builder.Append(Formater.PadRight(Formater.FormatNumber(creditsPaid), 160));
                builder.Append(" ");
                builder.Append(Formater.PadRight(Values.Items[Values.SC_ITEM].ToString(), 360, true));
                builder.Append(" -> Bankaccount\n");
            }

            foreach(Item item in payout)
            {
                while(item.Value >= 0.001 && currentInventoryIndex < inventories.Count)
                {
                    float amount = Utilities.TryPutItem(currentInventory.Inventory, item.ItemId, item.Value);
                    if (amount > 0)
                    {
                        builder.Append(Formater.PadRight(Formater.FormatNumber(amount), 160));
                        builder.Append(" ");
                        builder.Append(Formater.PadRight(Values.Items[item.ItemId].ToString(), 360, true));
                        builder.Append(" -> ");
                        builder.Append(currentInventory.Name);
                        builder.Append("\n");
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
            Utilities.ShowDialog(player.SteamUserId, "Claimed Bounty", builder.ToString());
        }

        private long PayCredits(IMyPlayer player, Hunter hunter)
        {
            foreach (Item item in hunter.ClaimableBounty)
            {
                if (item.ItemId.Equals(Values.SC_ITEM))
                {
                    long amount = (long)Math.Floor(item.Value);
                    if (amount > Config.Instance.MinPayout)
                    {
                        Utilities.PayPlayer(player, amount);
                        hunter.RemoveClaimable(item, amount);
                        return amount;
                    }
                    break;
                }
            }
            return 0;
        }
    }
}
