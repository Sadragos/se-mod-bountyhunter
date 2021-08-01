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
        public static List<LootboxSpawn> Lootboxes = new List<LootboxSpawn>();

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
            foreach(Bounty bounty in Bounties.Instance.Bounties)
            {
                if(bounty.RewardItem.Value >= 1) rewards.Add(bounty.RewardItem);
            }
            if(rewards.Count == 0)
            {
                SendMessage(player, "You don't have collected enough bounty to claim it.");
                return;
            }

            SpawnLootBox(player, rewards);
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

        private void SpawnLootBox(IMyPlayer player, List<Item> itemList)
        {
            float TotalVolume = 0;


            TotalVolume *= 1000f;
            Vector3D playerPos = player.GetPosition();
            MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(playerPos);
            bool isPlanet = false;
            float gravity = 0;
            double distance = 0;

            foreach(Item it in itemList)
            {
                TotalVolume += Utilities.ItemVolume(it.ItemId, (float)Math.Floor(it.Value));
                Logging.Instance.WriteLine("  " + Math.Floor(it.Value) + " " + it.ItemId);
            }

            Vector3D gravityVectorNorm = new Vector3D();
            if (planet != null)
            {
                IMyGravityProvider grav = planet.Components.Get<MyGravityProviderComponent>();
                gravityVectorNorm = Vector3D.Normalize(grav.GetWorldGravity(playerPos));
                gravity = grav.GetGravityMultiplier(playerPos);
                distance = Vector3D.Distance(playerPos, planet.PositionComp.WorldVolume.Center);
                isPlanet = gravity > 0.05;
            }


            string prefab = GetPrefab(TotalVolume);
            Logging.Instance.WriteLine(" Nearest Planet:  -> " + (planet != null ? (planet.Name + ", Distance from Center " + distance.ToString("0.0") + "m, Gravity: " + gravity.ToString("0.00")) : "none") + " using planet spawn " + isPlanet);
            Logging.Instance.WriteLine(" Total Volume of Items " + TotalVolume + " using prefab " + prefab);

            Vector3D spawn = isPlanet ? (playerPos - (gravityVectorNorm * 700)) : (playerPos + (Vector3D.Normalize(player.Controller.ControlledEntity.GetHeadMatrix(true).Forward) * 20));
            List<long> entities = MyVisualScriptLogicProvider.GetEntitiesInSphere(spawn, 10);
            entities.RemoveAll(l =>
            {
                VRage.ModAPI.IMyEntity e = MyAPIGateway.Entities.GetEntityById(l);
                return (e is MyPlanet || e.ToString().Contains("MyVoxelPhysics"));
            });
            bool free = entities.Count == 0;

            if (free)
            {
                // TODO bountie / rewards entfernen
                Lootboxes.Add(new LootboxSpawn()
                {
                    Items = itemList,
                    Owner = player,
                    PrefabName = prefab
                });

                Utilities.ShowChatMessage("Lootbox incoming. Good Luck!", player.IdentityId);
                MyVisualScriptLogicProvider.SpawnPrefabInGravity(
                    prefab,
                    spawn,
                    playerPos,
                    player.IdentityId);
            }
            else
            {
                Logging.Instance.WriteLine(" Could not spawn Loot, blocked by");
                foreach (long l in entities)
                {
                    VRage.ModAPI.IMyEntity e = MyAPIGateway.Entities.GetEntityById(l);
                    Logging.Instance.WriteLine("  " + e.DisplayName + " ( " + e.ToString() + ")");
                }
                Utilities.ShowChatMessage("Spawnposition blocked. Make sure you have enough space " + (isPlanet ? "above" : "in front of") + " you.", player.IdentityId);
            }
        }

        public static string GetPrefab(float volume)
        {
            if (volume < 125f * MyAPIGateway.Session.SessionSettings.BlocksInventorySizeMultiplier) return "SmallLootBox";
            if (volume < 3375f * MyAPIGateway.Session.SessionSettings.BlocksInventorySizeMultiplier) return "MediumLootBox";
            if (volume < 15625f * MyAPIGateway.Session.SessionSettings.BlocksInventorySizeMultiplier) return "LargeLootBox";
            return "MassiveLootBox";
        }
    }

    public class LootboxSpawn
    {
        public List<Item> Items;
        public IMyPlayer Owner;
        public string PrefabName;
    }
}
