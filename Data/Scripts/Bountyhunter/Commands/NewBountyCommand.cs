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
            if(!float.TryParse(arguments[3], out rewardItem.Value) || Math.Floor(rewardItem.Value) <= 0)
            {
                WrongArguments(player);
                return;
            }
            rewardItem.Value = (float)Math.Floor(rewardItem.Value);

            List<ItemConfig> foundItems = Values.FindItemFuzzy(arguments[4]);
            if(foundItems.Count != 1)
            {
                SendMessage(player, "Found none or too many Items matching " + arguments[4]+".");
                return;
            }
            ItemConfig payment = foundItems[0];
            rewardItem.ItemId = payment.ItemId;

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
                if(!float.TryParse(arguments[5], out bounty.Count) || bounty.Count <= 0)
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
            
            if(!string.IsNullOrEmpty(me.FactionTag))
            {
                Participants.GetFaction(me.FactionTag).BountyPlaced += bountyValue;
            }

            SendMessage(player, "You set a bounty of " + Formater.FormatNumber(rewardItem.Value) + " " + payment.ToString() + " on " + targetString + ". " + takenFrom);

            if(Config.Instance.AnnouncyBounties)
            {
                string message = "";
                string clientName = bounty.HideClient ? "Anonymous" : bounty.Client;

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

        public override string ArgumentDescription => "<kill/damage> <player/faction> <targetName> <amount> <payment item> [amount of kills/damage]";
        
    }
}
