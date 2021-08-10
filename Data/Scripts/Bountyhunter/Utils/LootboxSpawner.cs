using Bountyhunter.Store.Proto;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRageMath;

namespace Bountyhunter.Utils
{
    class LootboxSpawner
    {
        public static List<LootboxSpawn> Lootboxes = new List<LootboxSpawn>();

        private static IMyCubeGrid Grid = null;
        private static List<IMySlimBlock> GridBlocks = new List<IMySlimBlock>();
        private static List<IMyCargoContainer> Container = new List<IMyCargoContainer>();
        private static List<PrefabSpawn> AvailableContainers = new List<PrefabSpawn>()
        {
            new PrefabSpawn("SmallLootBox", 125f * MyAPIGateway.Session.SessionSettings.BlocksInventorySizeMultiplier),
            new PrefabSpawn("MediumLootBox", 3375f * MyAPIGateway.Session.SessionSettings.BlocksInventorySizeMultiplier),
            new PrefabSpawn("LargeLootBox", 15625f * MyAPIGateway.Session.SessionSettings.BlocksInventorySizeMultiplier),
            new PrefabSpawn("MassiveLootBox", 421000f * MyAPIGateway.Session.SessionSettings.BlocksInventorySizeMultiplier)
        };
        

        public static float MaxBoxSize { get { return AvailableContainers.MaxBy(p => p.Capacity).Capacity;  } }
        

        public static void SpawnLootBox(IMyPlayer player, List<Item> itemList)
        {
            float TotalVolume = 0;


            Vector3D playerPos = player.GetPosition();
            MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(playerPos);
            bool isPlanet = false;
            float gravity = 0;
            double distance = 0;

            foreach (Item it in itemList)
            {
                TotalVolume += Utilities.ItemVolume(it.ItemId, (float)Math.Floor(it.Value));
                Logging.Instance.WriteLine("  " + Math.Floor(it.Value) + " " + it.ItemId);
            }
            TotalVolume *= 1000f;

            Vector3D gravityVectorNorm = new Vector3D();
            if (planet != null)
            {
                IMyGravityProvider grav = planet.Components.Get<MyGravityProviderComponent>();
                gravityVectorNorm = Vector3D.Normalize(grav.GetWorldGravity(playerPos));
                gravity = grav.GetGravityMultiplier(playerPos);
                distance = Vector3D.Distance(playerPos, planet.PositionComp.WorldVolume.Center);
                isPlanet = gravity > 0.05;
            }


            PrefabSpawn prefab = GetPrefab(TotalVolume);
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
                
                Lootboxes.Add(new LootboxSpawn()
                {
                    Items = itemList,
                    Owner = player,
                    Prefab = prefab
                });

                Utilities.ShowChatMessage("Bountybox incoming.", player.IdentityId);
                MyVisualScriptLogicProvider.SpawnPrefabInGravity(
                    prefab.Name,
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

        public static PrefabSpawn GetPrefab(float volume)
        {
            PrefabSpawn prefabSpawn = AvailableContainers.Find(ac => ac.Capacity >= volume);
            if (prefabSpawn != null) return prefabSpawn;
            return AvailableContainers[AvailableContainers.Count - 1];
        }

        public static void NewSpawn(long entityId, string prefabName)
        {
            if (Lootboxes.Count == 0) return;

            try
            {
                Grid = null;
                Grid = MyAPIGateway.Entities.GetEntityById(entityId) as IMyCubeGrid;
                if (Grid != null && Grid.Physics != null)
                {
                    LootboxSpawn spawn = Lootboxes.Find(lb => (Grid.BigOwners.Contains(lb.Owner.IdentityId) || Grid.SmallOwners.Contains(lb.Owner.IdentityId)) && lb.Prefab.Name.Equals(prefabName));
                    if (spawn == null) return;

                    Logging.Instance.WriteLine(" found Bountybox for " + spawn.Owner.DisplayName);
                    Container.Clear();
                    GridBlocks.Clear();
                    Grid.GetBlocks(GridBlocks);

                    foreach (var block in GridBlocks)
                    {
                        if (block.FatBlock != null && block.FatBlock is IMyCargoContainer)
                        {
                            var cargo = block.FatBlock as IMyCargoContainer;
                            if (cargo != null && !cargo.MarkedForClose && cargo.IsWorking)
                            {
                                var inventory = cargo.GetInventory();
                                if (cargo.GetInventory() != null)
                                {
                                    Container.Add(cargo);
                                }
                            }
                        }
                    }

                    Lootboxes.Remove(spawn);
                    Container[0].CustomName = spawn.Owner.DisplayName + " Bountybox";

                    IMyInventory inven = Container[0].GetInventory();
                    foreach (Item item in spawn.Items)
                    {
                        float added = Utilities.TryPutItem(Container[0].GetInventory(), item.ItemId, item.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine(e.ToString());
            }
        }
    }

    public class LootboxSpawn
    {
        public List<Item> Items;
        public IMyPlayer Owner;
        public PrefabSpawn Prefab;
    }

    public class PrefabSpawn
    {
        public string Name;
        public float Capacity;

        public PrefabSpawn(string name, float capacity)
        {
            Name = name;
            Capacity = capacity;
        }
    }
}
