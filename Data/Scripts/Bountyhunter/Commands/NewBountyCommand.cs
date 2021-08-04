using Bountyhunter.Store;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
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
            Hunter me = Participants.GetPlayer(player);

            Bounty bounty = new Bounty();
            bounty.Client = player.DisplayName;
            bounty.Partial = true;

            // Bounty Type
            bounty.BountyType = arguments[0].ToLower().Equals("kill") ? EBountyType.Kill : EBountyType.Damage;

            // Target Type
            ETargetType TargetType = arguments[1].ToLower().Equals("player") ? ETargetType.Player : ETargetType.Faction;

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
                IMyPlayer targetPlayer = Utils.Utilities.GetPlayer(targetString);
                if(targetPlayer == null)
                {
                    SendMessage(player, "The Player " + targetString + " could not be found.");
                    return;
                }
                if(!Config.Instance.PlaceBountiesOnSelf && targetPlayer.SteamUserId.Equals(player.SteamUserId))
                {
                    SendMessage(player, "You can't place a bounty on yourself.");
                    return;
                }
            } else if (TargetType.Equals(ETargetType.Faction))
            {
                IMyFaction targetFaction = Utils.Utilities.GetFactionByTag(targetString);
                if (targetFaction == null)
                {
                    SendMessage(player, "No Faction with the Tag " + targetString + " could be found.");
                    return;
                }
                if(!Config.Instance.PlaceBountiesOnAllies && !targetFaction.IsEnemy(player.IdentityId))
                {
                    SendMessage(player, "You are not at war with this Faction and can't set a bounty.");
                    return;
                }
            }


            // Payment
            Item rewardItem = new Item();
            if(!float.TryParse(arguments[3], out rewardItem.Value) || Math.Floor(rewardItem.Value) <= 0)
            {
                WrongArguments(player);
                return;
            }
            rewardItem.Value = (float)Math.Floor(rewardItem.Value);

            List<BountyItem> foundItems = Values.FindItemFuzzy(arguments[4]);
            if(foundItems.Count != 1)
            {
                SendMessage(player, "Found none or too many Items matching " + arguments[4]+".");
                return;
            }
            BountyItem payment = foundItems[0];
            rewardItem.ItemId = payment.ItemId;

            bool isCredits = rewardItem.ItemId.Equals(Utils.Utilities.SC_ITEM);

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
            bounty.RecalculateRemainingCurrency();

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
            switch(TargetType)
            {
                case ETargetType.Faction:
                    Faction fTarget = Participants.GetFaction(targetString);
                    fTarget.Bounties.Add(bounty);
                    break;

                case ETargetType.Player:
                    Hunter pTarget = Participants.GetPlayer(targetString);
                    pTarget.Bounties.Add(bounty);
                    break;
            }
            
            me.BountyPlaced += rewardItem.Value * payment.Value;
            // TODO Statistik für Faction setzen
            // TODO Ankündigen
            SendMessage(player, "You set a bounty of " + rewardItem.Value + " " + payment.ToString() + " on " + targetString + ". " + takenFrom);

        }

        public override string ArgumentDescription()
        {
            return "<kill/damage> <player/faction> <targetName> <amount> <payment item> [amount of kills/damage]";
        }
    }
}
