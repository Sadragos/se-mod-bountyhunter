using Bountyhunter.Store;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Bountyhunter.Commands
{
    class NewBountyCommand : AbstactCommandHandler
    {

        public NewBountyCommand() : base("new") {

        }

        // TODO a little more logging maybe?
        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if(arguments.Length < 5)
            {
                WrongArguments(player);
                return;
            }
            Hunter me = Participants.GetPlayer(player.Identity);

            if(me.LastBountySet != null && me.LastBountySet.AddSeconds(Config.Instance.BountyDelaySeconds) > DateTime.Now)
            {
                int seconds = (int)(me.LastBountySet.AddSeconds(Config.Instance.BountyDelaySeconds) - DateTime.Now).TotalSeconds;
                SendMessage(player, "You have to wait " + seconds + " seconds before you can set a new Bounty.");
                return;
            }

            Bounty bounty = new Bounty();
            bounty.Client = player.DisplayName;
            bounty.Partial = true;

            // Bounty Type
            bounty.BountyType = arguments[0].ToLower().Equals("kill") || arguments[0].ToLower().Equals("k") ? EBountyType.Kill : EBountyType.Damage;

            // Target Type
            ETargetType TargetType = arguments[1].ToLower().Equals("player") || arguments[1].ToLower().Equals("p") ? ETargetType.Player : ETargetType.Faction;

            if (TargetType.Equals(ETargetType.Player) && !Config.Instance.EnablePlayerBounties)
            {
                SendMessage(player, "Bounties on Players are disabled.");
                return;
            }
            else if (TargetType.Equals(ETargetType.Faction) && !Config.Instance.EnableFactionBounties)
            {
                SendMessage(player, "Bounties on Factions are disabled.");
                return;
            }

            // Validate Target
            string targetString = arguments[2];
            if(TargetType.Equals(ETargetType.Player))
            {
                List<IMyIdentity> list = Utilities.GetPlayerIdentityFuzzy(targetString);
                StringBuilder builder = new StringBuilder();
                if (list.Count != 1)
                {
                    builder.Append("No player with that name found.");
                    if (list.Count > 1)
                    {
                        builder.Append(" Did you mean...\n");
                        foreach (IMyIdentity id in list)
                        {
                            builder.Append("- ");
                            builder.Append(id.DisplayName);
                            builder.Append("\n");
                        }
                    }
                    SendMessage(player, builder.ToString());
                    return;
                }
                IMyIdentity targetPlayer = list.FirstOrDefault();
                if (targetPlayer == null)
                {
                    SendMessage(player, "The Player " + targetString + " could not be found.");
                    return;
                }
                if(!Config.Instance.PlaceBountiesOnSelf && targetPlayer.IdentityId.Equals(player.IdentityId))
                {
                    SendMessage(player, "You can't place a bounty on yourself.");
                    return;
                }
                targetString = targetPlayer.DisplayName;
            } else if (TargetType.Equals(ETargetType.Faction))
            {
                IMyFaction targetFaction = Utilities.GetFactionByTag(targetString);
                if (targetFaction == null || targetFaction.IsEveryoneNpc())
                {
                    SendMessage(player, "No Faction with the Tag " + targetString + " could be found.");
                    return;
                }
                if(!Config.Instance.PlaceBountiesOnAllies && !targetFaction.IsEnemy(player.IdentityId))
                {
                    SendMessage(player, "You are not at war with this Faction and can't set a bounty.");
                    return;
                }
                targetString = targetFaction.Tag;
            }


            // Payment
            BountyItem rewardItem = new BountyItem();
            rewardItem.Value = Utilities.ParseNumber(arguments[3]);
            if(Math.Floor(rewardItem.Value) <= 0)
            {
                WrongArguments(player);
                return;
            }
            rewardItem.Value = (float)Math.Floor(rewardItem.Value);

            if(rewardItem.Value < Config.Instance.MinItemAmount || rewardItem.Value > Config.Instance.MaxItemAmount)
            {
                SendMessage(player, "You have to set an amount between " + Config.Instance.MinItemAmount + " and " + Config.Instance.MaxItemAmount + ".");
                return;
            }

            List<ItemConfig> foundItems = Values.FindItemFuzzy(arguments[4]);
            if(foundItems.Count != 1)
            {
                SendMessage(player, "Found none or too many Items matching " + arguments[4]+".");
                return;
            }
            ItemConfig payment = foundItems[0];
            rewardItem.ItemId = payment.ItemId;

            if(payment.Value < Config.Instance.FloatAmountBuffer)
            {
                SendMessage(player, payment.ToString() + " is currenty worth nothing and can not be used as Bounty.");
                return;
            }

            bool isCredits = rewardItem.ItemId.Equals(Values.SC_ITEM);

            if(!isCredits && !Config.Instance.EnableItemBounties)
            {
                SendMessage(player, "Items can not be used as Bounty currently.");
                return;
            }
            if(!payment.AllowedAsBounty)
            {
                SendMessage(player, "This Item can not be used as Bounty currently.");
                return;
            }

            if(!rewardItem.HasFractions)
            {
                rewardItem.Value = (float)Math.Round(rewardItem.Value);
            }
            bounty.RewardItem = rewardItem;
            bounty.RecalculateWorth();

            // Get Count / requirement
            if(arguments.Length == 6)
            {
                bounty.Count = Utilities.ParseNumber(arguments[5]);
                if (bounty.Count <= 0)
                {
                    WrongArguments(player);
                    return;
                }

                if (bounty.BountyType.Equals(EBountyType.Kill))
                {
                    bounty.Count = (float)Math.Ceiling(bounty.Count);
                }
            } else
            {
                if (bounty.BountyType.Equals(EBountyType.Kill))
                {
                    bounty.Count = 1;
                } else if (bounty.BountyType.Equals(EBountyType.Damage))
                {
                    bounty.Count = rewardItem.Value * payment.Value;
                }
            }


            // Validate player has Items and charge him
            bool paid = false;
            string takenFrom = "Payment was taken from your Bankaccount.";
            if(isCredits && Config.Instance.TakeCreditFromBank)
            {
                long balance;
                if(player.TryGetBalanceInfo(out balance) && balance >= rewardItem.Value)
                {
                    paid = true;
                    Utilities.PayPlayer(player, -((long)rewardItem.Value));
                }
            }

            if (!paid)
            {
                IMyInventory myInventory = player.Character.GetInventory(0);
                paid = Utilities.TryTakeItem(myInventory, rewardItem.ItemId, rewardItem.Value);
                takenFrom = "Payment was taken from your Inventory.";
            }

            if(!paid)
            {
                List<IMyCargoContainer> cargos = Utilities.GetCargoNearPlayer(player);
                foreach(IMyCargoContainer container in cargos)
                {
                    paid = Utilities.TryTakeItem(container.GetInventory(), rewardItem.ItemId, rewardItem.Value);
                    if (paid)
                    {
                        takenFrom = "Payment was taken from " + container.CustomName + " on " + container.CubeGrid.DisplayName;
                        break;
                    }
                }
                
            }
            if (!paid)
            {
                SendMessage(player, "You dont have " + rewardItem.Value + " " + payment.ToString());
                return;
            }


            // Finally! Set the bounty!
            float bountyValue = rewardItem.Value * payment.Value;
            bool existingBounties = false;
            switch (TargetType)
            {
                case ETargetType.Faction:
                    Faction fTarget = Participants.GetFaction(targetString);
                    existingBounties = fTarget.Bounties.Count > 0;
                    fTarget.Bounties.Add(bounty);
                    fTarget.BountyReceived += bountyValue;
                    break;

                case ETargetType.Player:
                    Hunter pTarget = Participants.GetPlayer(targetString);
                    existingBounties = pTarget.Bounties.Count > 0;
                    pTarget.Bounties.Add(bounty);
                    pTarget.BountyReceived += bountyValue;
                    break;
            }
            
            me.BountyPlaced += bountyValue;

            Utilities.ShowDialog(player.SteamUserId, "Bounty placed", "You set a bounty of " + Formater.FormatNumber(rewardItem.Value) + " " + payment.ToString() + " on " + targetString + ". " + takenFrom);
            me.LastBountySet = DateTime.Now;

            if(Config.Instance.AnnouncyBounties)
            {
                string message = "";
                string clientName = Config.Instance.AnnouncyBountiesWithName ?  bounty.Client : "Someone";

                if (existingBounties)
                {
                    message = clientName + " has increased the bounty on " + targetString;
                    if(Config.Instance.AnnouncyBountiesDeatils)
                    {
                        message += " by " + Formater.FormatNumber(bounty.RewardItem.Value) + " " + payment.ToString() + " (" + Formater.FormatCurrency(bountyValue) + ").";
                    } else
                    {
                        message += ".";
                    }
                }
                else
                {
                    message = clientName + " has placed a new bounty ";
                    if (Config.Instance.AnnouncyBountiesDeatils)
                    {
                        message += " of " + Formater.FormatNumber(bounty.RewardItem.Value) + " " + payment.ToString() + " (" + Formater.FormatCurrency(bountyValue) + ")";
                    }

                    message += " on " + targetString + ".";
                }

                Utilities.ShowChatMessage(message);
            }

        }

        public override string ArgumentDescription => "<kill/damage> <player/faction> <targetName> <amount> <payment item> [amount of kills/damage]" +
            "\nThis command lets you place new bounties on someone." +
            "You can use 'k' or 'd' instead of kill or damage for the first argument. " +
            "You can use 'p' or 'f' instead of player or faction as the second argument. " +
            "\n'targetName' must be a faction tag for factions or the name of a player. For playernames a part of the name can suffice, if it clearly identifies one player." +
            "\n'amount' can be any whole number. You can also use smart numbers like 5k for 5000, 5kk for 5000000 or 7m for 7000000." +
            "\n'payment item' can be any itemname, a part of it or an item ID. If you can't find an item use '/bh value item SEARCH' to find the item and then use the ID. " +
            "This is especially usefull for stuff like Iron Ore and Iron Ingots - as both have the name Iron you have to use the ID or ingot/iron for example." +
            "\n'amount of kills/damage' This argument is optional and allows you to specify how much damage or how many kills have to be done to claim the full bounty. " +
            "This is usefull if you want to place a bounty on the next 10 kills or if you want to payout abountyvalue of 5k for 10k of damage. If this argument is not set kills default to 1 and damage defaults to the value the bounty." +
            "\nNote: The bounty will be withdrawn directly from you, your Bankaccount or any cargo around you, that you have access to. If you can't afford the bounty this command will fail." +
            "\n\nExample: Kill Sadragos 5 times and reward 500 Steelplates for each kill" +
            "\n/bh new k p Sadragos 2500 Steelplate 5" +
            "\nExample: Destroy Blocks worth 500k for the faction ADM and pay 100k Credits for that" +
            "\n/bh new d f ADM 100k credit 500k" +
            "\nExample: Destroy Blocks worth the same value as 500k Steelplates and earn 500k Steelplates" +
            "\n/bh new d f ADM 500k steelplate" +
            "\nExample: Kill Sadragos for 500 Credits" +
            "\n/bh new k p Sadragos 500 credit";
        
    }
}
