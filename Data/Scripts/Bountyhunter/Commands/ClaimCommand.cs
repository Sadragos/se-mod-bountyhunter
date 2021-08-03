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
            List<Item> rewards = new List<Item>();
            
            // TODO Temporary
            if(rewards.Count == 0)
            {
                SendMessage(player, "You don't have collected enough bounty to claim it.");
                return;
            }

            LootboxSpawner.SpawnLootBox(player, rewards);
        }

        private void PayoutInventory(IMyInventory inventory, List<Item> rewards)
        {
            foreach(Item it in rewards)
            {
                float valueToAdd = (float)Math.Floor(it.Value);
                float added = Utils.Utilities.TryPutItem(inventory, it.ItemId, valueToAdd);
                if (!added.Equals(valueToAdd)) break;
            }
        }
    }
}
