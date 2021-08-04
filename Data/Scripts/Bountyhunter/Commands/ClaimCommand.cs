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
            
            // TODO Temporary
            foreach(Bounty b in Participants.GetPlayer(player).Bounties)
            {
                rewards.Add(b.RewardItem);
            }
            if(rewards.Count == 0)
            {
                SendMessage(player, "You don't have collected enough bounty to claim it.");
                return;
            }

            if (Config.Instance.SpawnRewardDropPods)
            {
                
                PayoutLootbox(player);
            } else
            {
                PayoutInventory(player);
            }
        }

        private void PayoutLootbox(IMyPlayer player)
        {
            // TODO not working properly
            //LootboxSpawner.SpawnLootBox(player);
        }

        private void PayoutInventory(IMyPlayer player)
        {

        }
    }
}
