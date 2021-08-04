using System;
using System.IO;
using System.Text;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using Sandbox.Game.Weapons;
using IMyCubeBlock = VRage.Game.ModAPI.IMyCubeBlock;
using IMySlimBlock = VRage.Game.ModAPI.IMySlimBlock;
using VRage.Game.ModAPI.Interfaces;
using Sandbox.Common.ObjectBuilders;
using Bountyhunter.Data.Proto;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using IMyEntity = VRage.ModAPI.IMyEntity;
using IMyCubeGrid = VRage.Game.ModAPI.IMyCubeGrid;
using IMyInventory = VRage.Game.ModAPI.IMyInventory;
using IMyInventoryItem = VRage.Game.ModAPI.IMyInventoryItem;
using VRage;
using Bountyhunter.Store;
using VRageMath;

namespace Bountyhunter.Utils
{
    internal class Utilities
    {
        public static string SC_ITEM = "MyObjectBuilder_PhysicalObject/SpaceCredit";


        public static IMyIdentity EntityToIdentity(IMyEntity entity)
        {
            if (entity == null) { return null; }

            if (entity is IMyCharacter)
            {
                return CharacterToIdentity((IMyCharacter)entity);
            }
            else if (entity is IMyEngineerToolBase)
            {
                var tool = (IMyEngineerToolBase)entity;
                if (tool == null) { return null; }

                var toolOwner = MyAPIGateway.Entities.GetEntityById(tool.OwnerId);
                if (toolOwner == null) { return null; }

                var character = (IMyCharacter)toolOwner;
                if (character == null) { return null; }

                return CharacterToIdentity(character);
            }
            else if (entity is MyCubeBlock)
            {
                var block = (MyCubeBlock)entity;
                if (block == null) { return null; }
                return CubeBlockOwnerToIdentity(block);
            }
            else if (entity is IMyGunBaseUser)
            {
                var weapon = (IMyGunBaseUser)entity;
                if (weapon == null) { return null; }

                var weaponOwner = weapon.Owner;
                if (weaponOwner == null) { return null; }

                var character = (IMyCharacter)weaponOwner;
                if (character == null) { return null; }

                return CharacterToIdentity(character);
            }
            else if (entity is IMyCubeGrid)
            {
                return GridToIdentity((IMyCubeGrid)entity);
            } 
            else if(entity.GetType().Name == "MyMissile")
            {
                try
                {
                    MyObjectBuilder_Missile builder = (MyObjectBuilder_Missile)entity.GetObjectBuilder();
                    var identities = new List<IMyIdentity>();
                    MyAPIGateway.Players.GetAllIdentites(identities, i => i.IdentityId == builder.Owner);
                    return identities.FirstOrDefault();
                } catch (Exception e)
                {
                    Logging.Instance.WriteLine("Could not parse missileentity");
                }
            }

            return null;
        }

        public static IMyIdentity CharacterToIdentity(IMyCharacter character)
        {
            if (character == null) { return null; }
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, p => p.Character == character);
            var player = players.FirstOrDefault();
            if (player == null) { return null; }
            return player.Identity;
        }

        public static IMyIdentity CubeBlockOwnerToIdentity(IMyCubeBlock block)
        {
            if (block == null) { return null; }
            var identities = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(identities, i => i.IdentityId == block.OwnerId);
            return identities.FirstOrDefault();
        }

        public static void ShowChatMessage(string message, long playerid = 0)
        {
            //this takes the death message and sends it out to all players
            Logging.Instance.WriteLine("ShowMessage " + message);

            //only the server should do this
            if (!MyAPIGateway.Multiplayer.IsServer)
            {
                return;
            }
            MyVisualScriptLogicProvider.SendChatMessageColored(message, Config.Instance.BroadcastNameRealColor, (playerid != 0 ? "~" : "")+ Config.Instance.BroadcastName, playerid);
        }

        public static IMyIdentity GridToIdentity(IMyCubeGrid grid)
        {
            if (grid == null) { return null; }

            var gridOwnerId = grid.BigOwners.FirstOrDefault();
            var identities = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(identities, i => i.IdentityId == gridOwnerId);
            var ownerIdentity = identities.FirstOrDefault();
            if (ownerIdentity != null) { return ownerIdentity; }

            // can't find owner, go by the first built by
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            grid.GetBlocks(blocks);
            var block = blocks.FirstOrDefault();
            if (block == null || !(block is MyCubeBlock)) { return null; }
            return CubeBlockBuiltByToIdentity(((MyCubeBlock)block).BuiltBy);
        }

        public static IMyIdentity CubeBlockBuiltByToIdentity(long builtBy)
        {
            var identities = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(identities, i => i.IdentityId == builtBy);
            return identities.FirstOrDefault();
        }

        public static string TagPlayerName(IMyIdentity identity)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(identity.IdentityId);
            return (faction != null) ? (faction.Tag + "." + identity.DisplayName) : identity.DisplayName;
        }

        public static IMyPlayer IdentityToPlayer(IMyIdentity identity)
        {
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, i => i.IdentityId == identity.IdentityId);
            return players.FirstOrDefault();
        }

        public static byte[] MessageToBytes(ClientServerMessage data)
        {
            try
            {
                string itemMessage = MyAPIGateway.Utilities.SerializeToXML(data);
                byte[] itemData = Encoding.UTF8.GetBytes(itemMessage);
                return itemData;
            } catch (Exception e)
            {
                Logging.Instance.WriteLine(e.ToString());
                return null;
            }
        }

        public static long PayFaction(IMyFaction faction, long amount)
        {
            if (amount < 0)
            {
                long balance = 0;
                if (faction.TryGetBalanceInfo(out balance))
                {
                    balance = Math.Max(-balance, amount);
                    faction.RequestChangeBalance(balance);
                    return balance;
                }
                return 0;
            }
            else
            {
                faction.RequestChangeBalance(amount);
                return amount;
            }
        }

        public static long PayPlayer(IMyPlayer player, long amount)
        {
            if(amount < 0)
            {
                long balance = 0;
                if(player.TryGetBalanceInfo(out balance))
                {
                    balance = Math.Max(-balance, amount);
                    player.RequestChangeBalance(balance);
                    return balance;
                }
                return 0;
            } else
            {
                player.RequestChangeBalance(amount);
                return amount;
            }
        }

        public static void SendMessageToClient(ClientServerMessage data, ulong steamid)
        {
            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
            {
                MyAPIGateway.Multiplayer.SendMessageTo(Core.CLIENT_ID, MessageToBytes(data), steamid);
            });
        }

        public static ClientServerMessage BytesToMessage(byte[] bytes)
        {
            try
            {
                string itemMessage = Encoding.UTF8.GetString(bytes);
                ClientServerMessage itemData = MyAPIGateway.Utilities.SerializeFromXML<ClientServerMessage>(itemMessage);
                return itemData;
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine(e.ToString());
                return null;
            }
        }

        public static string CurrentTimestamp()
        {
            return DateTime.Now.ToString("dd.MM.yy HH:mm:ss");
        }

        public static IMyPlayer GetPlayer(ulong steamid)
        {
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, i => i.SteamUserId == steamid);
            return players.FirstOrDefault();
        }

        public static IMyPlayer GetPlayer(string name)
        {
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, i => i.DisplayName.ToLower().Equals(name.ToLower()));
            return players.FirstOrDefault();
        }

        public static IMyFaction GetFactionByTag(string tag)
        {
            return MyAPIGateway.Session.Factions.TryGetFactionByTag(tag);
        }

        public static bool TryTakeItem(IMyInventory inventory, string itemId, float amount)
        {
            List<IMyInventoryItem> items = inventory.GetItems();
            foreach (IMyInventoryItem item in items)
            {
                if (item.Content.GetId().ToString().Equals(itemId))
                {
                    if ((float)item.Amount >= amount)
                    {
                        MyObjectBuilder_PhysicalObject builder = (MyObjectBuilder_PhysicalObject)item.Content;

                        if (builder == null) continue;
                        inventory.RemoveItemsOfType((VRage.MyFixedPoint)Math.Abs(amount), builder);
                        return true;
                    }
                }
            }
            return false;
        }

        // Liefert zurück, wieviele Items ins Inventar gepackt wurden
        public static float TryPutItem(IMyInventory inventory, string itemId, float amount, bool partial = true, float max = 1000000)
        {
            MyFixedPoint fixedAmount = MyFixedPoint.Floor((MyFixedPoint) amount);
            MyObjectBuilder_PhysicalObject builder = GetItemBuilder(itemId);
            float volumeLeft = (float)inventory.MaxVolume - (float)inventory.CurrentVolume;
            float volume = ItemVolume(itemId);
            float totalVolume = (float)fixedAmount * volume;
            Logging.Instance.WriteLine("Adding " + fixedAmount + " " + itemId + " with a volume of " + totalVolume.ToString());
            if (totalVolume <= volumeLeft)
            {
                Logging.Instance.WriteLine("- Everything fits ");
                inventory.AddItems(fixedAmount, builder);
                return amount;
            }
            if (totalVolume > volumeLeft && !partial)
            {
                Logging.Instance.WriteLine("- Doesnt fit ");
                return 0;
            }

            fixedAmount = MyFixedPoint.Ceiling((MyFixedPoint)Math.Min(max, (float)Math.Floor(volumeLeft / volume)));
            Logging.Instance.WriteLine("- Adding " + fixedAmount + " with a volume of " + (fixedAmount * volume));
            inventory.AddItems(fixedAmount, builder);

            return (float) fixedAmount;
        }


        public static MyObjectBuilder_PhysicalObject GetItemBuilder(string itemId)
        {
            string[] parts = itemId.Split('/');
            string typeId = parts[0];
            string subtypeId = parts[1];
            switch (typeId)
            {
                case "MyObjectBuilder_Component":
                    return new MyObjectBuilder_Component() { SubtypeName = subtypeId };
                case "MyObjectBuilder_AmmoMagazine":
                    return new MyObjectBuilder_AmmoMagazine() { SubtypeName = subtypeId };
                case "MyObjectBuilder_Ingot":
                    return new MyObjectBuilder_Ingot() { SubtypeName = subtypeId };
                case "MyObjectBuilder_Ore":
                    return new MyObjectBuilder_Ore() { SubtypeName = subtypeId };
                case "MyObjectBuilder_PhysicalObject":
                    return new MyObjectBuilder_PhysicalObject() { SubtypeName = subtypeId };
                case "MyObjectBuilder_ConsumableItem":
                    return new MyObjectBuilder_ConsumableItem() { SubtypeName = subtypeId };
                default:
                    Logging.Instance.WriteLine("No builder for " + itemId);
                    return null;
            }
        }

        public static float ItemVolume(string itemId, float amount = 1)
        {
            MyObjectBuilder_PhysicalObject builder = GetItemBuilder(itemId);
            MyItemType type = new MyItemType(builder.TypeId, builder.SubtypeId);
            return type.GetItemInfo().Volume * amount;
        }

        public static List<IMyCubeGrid> GetGridsNearPlayer(IMyPlayer player, float range = 5)
        {
            List<IMyCubeGrid> grids = new List<IMyCubeGrid>();

            BoundingSphereD sphere = new BoundingSphereD(player.GetPosition(), range);
            foreach (IMyEntity ent in MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere))
            {
                if (!(ent is IMyCubeGrid)) continue;
                grids.Add(ent as IMyCubeGrid);
            }

            return grids;
        }

        public static List<IMyCargoContainer> GetCargoNearPlayer(IMyPlayer player, float range = 5)
        {
            List<IMyCargoContainer> containers = new List<IMyCargoContainer>();
            foreach (MyCubeGrid grid in GetGridsNearPlayer(player, range))
            {
                foreach(MyCubeBlock block in grid.Inventories)
                {
                    if (!(block is IMyCargoContainer)) continue;
                    if(block.OwnerId == player.IdentityId || block.GetUserRelationToOwner(player.IdentityId).HasFlag(MyRelationsBetweenPlayerAndBlock.FactionShare))
                    {
                        containers.Add(block as IMyCargoContainer);
                    }
                }
            }
            return containers;
        }
    }
}