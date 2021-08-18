using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using System.Xml.Serialization;
using static Sandbox.Definitions.MyCubeBlockDefinition;
using ProtoBuf;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using Bountyhunter.Store.Proto.Files;
using VRage.Game;

namespace Bountyhunter.Store
{
    public class Values
    {
        public static string SC_ITEM = "MyObjectBuilder_PhysicalObject/SpaceCredit";
        public static string COMPUTER_ITEM = "MyObjectBuilder_Component/Computer";

        public static FileValues Instance;

        public static Dictionary<string, ItemConfig> Items = new Dictionary<string, ItemConfig>();
        public static Dictionary<string, BlockConfig> Blocks = new Dictionary<string, BlockConfig>();

        public static object CubeSize { get; private set; }

        public static void Load()
        {
            bool initNewConfig = true;
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage("ItemBlockValues.xml", typeof(FileValues)))
            {
                try
                {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("ItemBlockValues.xml", typeof(FileValues));
                    var xmlData = reader.ReadToEnd();
                    Instance = MyAPIGateway.Utilities.SerializeFromXML<FileValues>(xmlData);
                    reader.Dispose();
                    Logging.Instance.WriteLine("ItemBlockValues found and loaded");
                    initNewConfig = false;
                }
                catch (Exception e)
                {
                    Logging.Instance.WriteLine("ItemBlockValues loading failed");
                }
            }

            ValidateData();
            if(initNewConfig)
            {
                InitalValues();
                CalculateOres();
                CalculateComponent();
                CalculateBlocks();
                Logging.Instance.WriteLine("Initialized new Config with default values.");
            }
            Instance2Dict();
            Save();
        }

        private static void InitalValues()
        {
            Instance.ItemValues.Find(item => item.ItemId.Equals(SC_ITEM)).Value = 1;
            Instance.ItemValues.Find(item => item.ItemId.Equals("MyObjectBuilder_Ingot/Stone")).Value = 0.25f;
            Instance.ItemValues.Find(item => item.ItemId.Equals("MyObjectBuilder_Ingot/Iron")).Value = 0.14f;
            Instance.ItemValues.Find(item => item.ItemId.Equals("MyObjectBuilder_Ingot/Nickel")).Value = 0.3f;
            Instance.ItemValues.Find(item => item.ItemId.Equals("MyObjectBuilder_Ingot/Cobalt")).Value = 1.9f;
            Instance.ItemValues.Find(item => item.ItemId.Equals("MyObjectBuilder_Ingot/Magnesium")).Value = 55.8f;
            Instance.ItemValues.Find(item => item.ItemId.Equals("MyObjectBuilder_Ingot/Silicon")).Value = 0.16f;
            Instance.ItemValues.Find(item => item.ItemId.Equals("MyObjectBuilder_Ingot/Silver")).Value = 6f;
            Instance.ItemValues.Find(item => item.ItemId.Equals("MyObjectBuilder_Ingot/Gold")).Value = 52f;
            Instance.ItemValues.Find(item => item.ItemId.Equals("MyObjectBuilder_Ingot/Platinum")).Value = 300f;
            Instance.ItemValues.Find(item => item.ItemId.Equals("MyObjectBuilder_Ingot/Uranium")).Value = 150f;
            Instance.ItemValues.Find(item => item.ItemId.Equals("MyObjectBuilder_Ore/Ice")).Value = 0.1f;
        }

        private static void ValidateData()
        {
            if (Instance == null) Instance = new FileValues();

            PopulateMissing(Instance);
        }

        private static void Instance2Dict()
        {
            Blocks.Clear();
            foreach (BlockConfig b in Instance.BlockValues)
            {
                if (Blocks.ContainsKey(b.BlockId))
                {
                    Logging.Instance.WriteLine("WARNING Duplicate BlockId " + b.BlockId);
                    continue;
                }
                Blocks.Add(b.BlockId, b);
            }

            Items.Clear();
            foreach (ItemConfig i in Instance.ItemValues)
            {
                if (Items.ContainsKey(i.ItemId))
                {
                    Logging.Instance.WriteLine("WARNING Duplicate ItemId " + i.ItemId);
                    continue;
                }
                Items.Add(i.ItemId, i);
            }
        }

        public static void Save()
        {
            try
            {
                Logging.Instance.WriteLine("Serializing ItemBlockValues to XML... ");
                string xml = MyAPIGateway.Utilities.SerializeToXML(Instance);
                Logging.Instance.WriteLine("Writing ItemBlockValues to disk... ");
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("ItemBlockValues.xml", typeof(FileValues));
                writer.Write(xml);
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine("Error saving ItemBlockValues XML!" + e.StackTrace);
            }
        }

        private static void PopulateMissing(FileValues instance)
        {
            var allDefs = MyDefinitionManager.Static.GetAllDefinitions();


            Logging.Instance.WriteLine("Loading Blueprints...");
            Dictionary<string, List<Item>> blueprints = new Dictionary<string, List<Item>>();
            foreach (var blueprint in MyDefinitionManager.Static.GetBlueprintDefinitions().OfType<MyBlueprintDefinition>())
            {
                List<Item> components = new List<Item>();

                if (blueprint.Results.Length == 0) continue;

                MyBlueprintDefinitionBase.Item result = blueprint.Results[0];
                float amount = (float) result.Amount;
                float multi = 1f / amount;
                string targetId = result.Id.ToString();


                if(blueprints.ContainsKey(targetId))
                {
                    Logging.Instance.WriteLine("Warn: duplicate Blueprint for " + targetId + " -> " + blueprint.Id.ToString());
                    continue;
                }

                foreach (MyBlueprintDefinitionBase.Item comp in blueprint.Prerequisites)
                {
                    Item item = new Item(comp.Id.ToString(), ((float)comp.Amount * multi) / MyAPIGateway.Session.SessionSettings.AssemblerEfficiencyMultiplier);
                    components.Add(item);
                }

                blueprints.Add(targetId, components);

            }

            Logging.Instance.WriteLine("Reversing Ore Quotas...");
            List<string> entries = new List<string>(blueprints.Keys);
            foreach(string bp in entries)
            {
                if(bp.StartsWith("MyObjectBuilder_Ingot"))
                {
                    List<Item> items = blueprints[bp];
                    if (items.Count == 0) continue;
                    string key = items[0].ItemId;
                    float amount = 1f / items[0].Value;
                    List<Item> reverse = new List<Item>()
                    {
                        new Item(bp, amount)
                    };
                    if (blueprints.ContainsKey(key))
                    {
                        Logging.Instance.WriteLine("Warn: duplicate Blueprint for " + key);
                        continue;
                    }
                    blueprints.Add(key, reverse);
                }
            }

            Logging.Instance.WriteLine("Find missing Components...");
            foreach (var componenet in allDefs.OfType<MyPhysicalItemDefinition>())
            {
                string key = componenet.Id.ToString();
                if (instance.ItemValues.Find(t => t.ItemId.Equals(key)) == null)
                {
                    ItemConfig item = new ItemConfig(key);
                    item.Alias.Add(componenet.DisplayNameText);
                    if(!key.StartsWith("MyObjectBuilder_Ingot")) blueprints.TryGetValue(key, out item.Components);

                    string clearName = GetClearName(key);
                    int nr = 1;
                    string itemId = clearName + nr.ToString("000");
                    while(Instance.ItemValues.Find(it => it.BountyId.Equals(itemId)) != null)
                    {
                        nr++;
                        itemId = clearName + nr.ToString("000");
                    }
                    item.BountyId = itemId;

                    instance.ItemValues.Add(item);
                }
            }

            Logging.Instance.WriteLine("Find missing Blocks...");
            foreach (var componenet in allDefs.OfType<MyCubeBlockDefinition>())
            {
                string key = componenet.Id.ToString();
                if (instance.BlockValues.Find(t => t.BlockId.Equals(key)) == null)
                {
                    string prefix = "[" + (componenet.CubeSize.Equals(MyCubeSize.Large) ? "L" : "S") +"] ";
                    BlockConfig bv = new BlockConfig(key);
                    bv.Alias.Add(prefix + componenet.DisplayNameText);

                    string clearName = GetClearName(key);
                    int nr = 1;
                    string itemId = clearName + nr.ToString("000");
                    while (Instance.BlockValues.Find(it => it.BountyId.Equals(itemId)) != null)
                    {
                        nr++;
                        itemId = clearName + nr.ToString("000");
                    }
                    bv.BountyId = itemId;

                    foreach (Component comp in componenet.Components)
                    {
                        Item iv = bv.Components.Find(t => t.ItemId.Equals(comp.Definition.Id.ToString()));
                        if (iv == null)
                        {
                            iv = new Item(comp.Definition.Id.ToString());
                            bv.Components.Add(iv);
                        }
                        iv.Value += comp.Count;
                    }

                    // Calculate Value of Block based on Components
                    foreach (Item iv in bv.Components)
                    {
                        ItemConfig ivWorth = instance.ItemValues.Find(t => t.ItemId.Equals(iv.ItemId));
                        if (ivWorth == null) continue;
                        bv.Value += iv.Value * ivWorth.Value;
                    }

                    instance.BlockValues.Add(bv);
                }
            }
            Instance2Dict();
        }

        public static string GetClearName(string input, int length = 3)
        {
            input = input.Replace("MyObjectBuilder_", "");
            input = input.Replace("Large", "");
            input = input.Replace("Small", "");
            input = input.Replace("Block", "");
            input = input.ToUpper();
            string[] parts = input.Split('/'); 
            string clearName = parts.Length > 1 && !parts[1].Equals("(null)") ? parts[1] : parts[0];
            clearName = clearName.Substring(0, Math.Min(length, clearName.Length));
            return clearName;
        }

        public static List<ItemConfig> FindItemFuzzy(string itemId)
        {
            itemId = itemId.ToUpper();

            ItemConfig conf = Instance.ItemValues.Find(it => it.BountyId.Equals(itemId));
            if (conf != null)
            {
                return new List<ItemConfig>() { conf };
            }

            return Instance.ItemValues.FindAll(item =>
                item.BountyId.Contains(itemId)
                || item.ItemId.ToUpper().Contains(itemId) 
                || (item.Alias != null && item.Alias.Find(alias => alias.ToUpper().Contains(itemId)) != null)
            );
        }

        public static ItemConfig FindItem(string itemId)
        {
            ItemConfig result;
            Items.TryGetValue(itemId, out result);
            return result;
        }

        public static float ItemValue(string itemId)
        {
            ItemConfig bi = FindItem(itemId);
            if (bi == null) return 0;
            return bi.Value;
        }

        public static List<BlockConfig> FindBlockFuzzy(string itemId)
        {
            itemId = itemId.ToUpper();
            BlockConfig conf = Instance.BlockValues.Find(bc => bc.BountyId.Equals(itemId));
            if(conf != null)
            {
                return new List<BlockConfig>() { conf };
            }
            return Instance.BlockValues.FindAll(item =>
                item.BountyId.Contains(itemId)
                || item.BlockId.ToUpper().Contains(itemId) 
                || (item.Alias != null && item.Alias.Find(alias => alias.ToUpper().Contains(itemId)) != null)
            );
        }

        public static BlockConfig FindBlock(string blockId)
        {
            BlockConfig result;
            Blocks.TryGetValue(blockId, out result);
            return result;
        }

        public static float BlockValue(string itemId)
        {
            BlockConfig bl = FindBlock(itemId);
            if (bl == null) return 0;
            return bl.Value;
        }

        public static void CalculateOres()
        {
            foreach(ItemConfig bi in Instance.ItemValues)
            {
                if (!bi.ItemId.StartsWith("MyObjectBuilder_Ore") || bi.Components == null || bi.Components.Count == 0) continue;
                bi.Value = bi.Components[0].Value * ItemValue(bi.Components[0].ItemId);
            }
        }

        public static void CalculateComponent()
        {
            foreach (ItemConfig bi in Instance.ItemValues)
            {
                if (bi.ItemId.StartsWith("MyObjectBuilder_Ore") || bi.ItemId.StartsWith("MyObjectBuilder_Ingot") || bi.Components == null) continue;
                bi.Value = 0;
                foreach(Item comp in bi.Components)
                {
                    bi.Value += ItemValue(comp.ItemId) * comp.Value;
                }
            }
        }

        public static void CalculateBlocks()
        {
            foreach (BlockConfig bl in Instance.BlockValues)
            {
                if (bl.Components == null) continue;
                bl.Value = 0;
                foreach (Item comp in bl.Components)
                {
                    ItemConfig valueComponent = FindItem(comp.ItemId);
                    if (valueComponent == null) continue;
                    bl.Value += valueComponent.Value * comp.Value;
                }
            }
        }

        public static PointValue CalculateValue(IMyCubeGrid grid, bool includeCargo = false)
        {
            Dictionary<string, int> components = new Dictionary<string, int>();
            List<IMySlimBlock> slimBlocks = new List<IMySlimBlock>();
            grid.GetBlocks(slimBlocks);
            double gridValue = 0;
            double cargoValue = 0;
            foreach (IMySlimBlock block in slimBlocks)
            {
                if (block.FatBlock == null) continue;
                gridValue += BlockValue(block.FatBlock.BlockDefinition.ToString());

                if (!includeCargo) continue;
                if (!(block.FatBlock is IMyTerminalBlock)) continue;

                IMyTerminalBlock term = block.FatBlock as IMyTerminalBlock;
                if (!term.HasInventory) continue;
                for(int inv = 0; inv < term.InventoryCount; inv++)
                {
                    IMyInventory myInventory = term.GetInventory(inv);
                    for(int it = 0; it < myInventory.ItemCount; it++)
                    {
                        VRage.Game.ModAPI.Ingame.MyInventoryItem? v = myInventory.GetItemAt(it);
                        if(v != null && v.HasValue)
                        cargoValue += (float)v.Value.Amount * ItemValue(v.Value.Type.ToString());
                    }
                }
            }
            return new PointValue() { CargoValue = cargoValue, GridValue = gridValue };
        }

        public static PointValue CalculateValue(IMyPlayer player, bool includeCargo = true)
        {
            PointValue value = new PointValue();
            IMyEntity cache;
            HashSet<long> grids = player.Grids;
            foreach (long l in grids)
            {
                if (MyAPIGateway.Entities.TryGetEntityById(l, out cache) && cache is IMyCubeGrid)
                {
                    value.Add(CalculateValue(cache as IMyCubeGrid, includeCargo));
                }
            }
            return value;
        }

        public struct PointValue
        {
            public double CargoValue;
            public double GridValue;

            public void Add(PointValue adder)
            {
                CargoValue += adder.CargoValue;
                GridValue += adder.GridValue;
            }
        }
    }
}
