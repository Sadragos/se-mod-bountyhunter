using Bountyhunter.Store;
using Bountyhunter.Store.Proto;
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

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if(arguments.Length < 5)
            {
                WrongArguments(player);
                return;
            }

            Bounty bounty = new Bounty();
            bounty.Client = player.DisplayName;
            bounty.Partial = true;

            // Target Type
            bounty.TargetType = arguments[0].ToLower().Equals("player") ? ETargetType.Player : ETargetType.Faction;

            // Validate Target
            string targetString = arguments[1];
            if(bounty.TargetType.Equals(ETargetType.Player))
            {
                IMyPlayer myPlayer = Utils.Utilities.GetPlayer(targetString);
                if(myPlayer == null)
                {
                    SendMessage(player, "The Player " + targetString + " could not be found.");
                    return;
                }
                bounty.Target = myPlayer.DisplayName;
            } else if (bounty.TargetType.Equals(ETargetType.Faction))
            {
                IMyFaction myFaction = Utils.Utilities.GetFactionByTag(targetString);
                if (myFaction == null)
                {
                    SendMessage(player, "No Faction with the Tag " + targetString + " could be found.");
                    return;
                }
                bounty.Target = myFaction.Tag;
            }

            // Bounty Type
            bounty.BountyType = arguments[2].ToLower().Equals("kill") ? EBountyType.Kill : EBountyType.Damage;

            // Payment
            Item rewardItem = new Item();
            
            if(!float.TryParse(arguments[3], out rewardItem.Value) || rewardItem.Value <= 0)
            {
                WrongArguments(player);
                return;
            }

            List<BountyItem> foundItems = Values.FindItemFuzzy(arguments[4]);
            if(foundItems.Count != 1)
            {
                SendMessage(player, "Found none or too many Items matching " + arguments[4]+".");
                return;
            }
            BountyItem payment = foundItems[0];
            rewardItem.ItemId = payment.ItemId;

            if(!rewardItem.ItemId.StartsWith("MyObjectBuilder_Ingot") && !rewardItem.ItemId.StartsWith("MyObjectBuilder_Ore"))
            {
                rewardItem.Value = (float)Math.Ceiling(rewardItem.Value);
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
            if(rewardItem.ItemId.Equals(Utils.Utilities.SC_ITEM))
            {
                long balance;
                if(player.TryGetBalanceInfo(out balance) && balance >= rewardItem.Value)
                {
                    paid = true;
                    Utils.Utilities.PayPlayer(player, -((long)rewardItem.Value));
                }
            }
            if (!paid)
            {
                IMyInventory myInventory = player.Character.GetInventory(0);
                paid = Utils.Utilities.TryTakeItem(myInventory, rewardItem.ItemId, rewardItem.Value);
            }
            if(!paid)
            {
                SendMessage(player, "You dont have " + rewardItem.Value + " " + payment.ToString());
                return;
            }


            // Finally! Set the bounty!
            Bounties.Instance.Bounties.Add(bounty);
            SendMessage(player, "You set a bounty of " + rewardItem.Value + " " + payment.ToString() + " on " + bounty.Target);

        }

        public override string ArgumentDescription()
        {
            return "<player/faction> <targetName> <kill/damage> <amount> <payment item> [amount of kills/damage]";
        }
    }
}
