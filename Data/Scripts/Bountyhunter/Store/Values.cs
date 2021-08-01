﻿using Sandbox.Definitions;
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

namespace Bountyhunter.Store
{
    public class Values
    {

        public static ValuesDefinition Instance;


        public static void Load()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage("ItemBlockValues.xml", typeof(ValuesDefinition)))
            {
                try
                {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("ItemBlockValues.xml", typeof(ValuesDefinition));
                    var xmlData = reader.ReadToEnd();
                    Instance = MyAPIGateway.Utilities.SerializeFromXML<ValuesDefinition>(xmlData);
                    reader.Dispose();
                    Logging.Instance.WriteLine("ItemBlockValues found and loaded");
                }
                catch (Exception e)
                {
                    Logging.Instance.WriteLine("ItemBlockValues loading failed");
                }
            }

            ValidateData();
        }

        private static void ValidateData()
        {
            if (Instance == null) Instance = new ValuesDefinition();

            PopulateMissing(Instance);

            Save();
        }

        public static void Save()
        {
            try
            {
                Logging.Instance.WriteLine("Serializing ItemBlockValues to XML... ");
                string xml = MyAPIGateway.Utilities.SerializeToXML(Instance);
                Logging.Instance.WriteLine("Writing ItemBlockValues to disk... ");
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("ItemBlockValues.xml", typeof(ValuesDefinition));
                writer.Write(xml);
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine("Error saving ItemBlockValues XML!" + e.StackTrace);
            }
        }

        private static void PopulateMissing(ValuesDefinition instance)
        {
            if (instance.ItemValues == null) instance.ItemValues = new List<BountyItem>();
            if (instance.BlockValues == null) instance.BlockValues = new List<Block>();

            var allDefs = MyDefinitionManager.Static.GetAllDefinitions();


            // Blueprints auslesen
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
                    Item item = new Item(comp.Id.ToString(), (float)comp.Amount * multi);
                    components.Add(item);
                }

                blueprints.Add(targetId, components);

            }

            // -> Ores/Ingots Umrechnen
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
                    blueprints.Add(key, reverse);
                }
            }

            // Find all missing Components
            foreach (var componenet in allDefs.OfType<MyPhysicalItemDefinition>())
            {
                string key = componenet.Id.ToString();
                if (instance.ItemValues.Find(t => t.ItemId.Equals(key)) == null)
                {
                    BountyItem item = new BountyItem(key);
                    item.Alias.Add(componenet.DisplayNameText);
                    if(!key.StartsWith("MyObjectBuilder_Ingot")) blueprints.TryGetValue(key, out item.Components);
                    instance.ItemValues.Add(item);
                }
            }

            // Find all Missing Blocks
            foreach (var componenet in allDefs.OfType<MyCubeBlockDefinition>())
            {
                string key = componenet.Id.ToString();
                if (instance.BlockValues.Find(t => t.BlockId.Equals(key)) == null)
                {
                    Block bv = new Block(key);
                    bv.Alias.Add(componenet.DisplayNameText);

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
                        BountyItem ivWorth = instance.ItemValues.Find(t => t.ItemId.Equals(iv.ItemId));
                        if (ivWorth == null) continue;
                        bv.Value += iv.Value * ivWorth.Value;
                    }

                    instance.BlockValues.Add(bv);
                }
            }
        }

        public static List<BountyItem> FindItemFuzzy(string itemId)
        {
            itemId = itemId.ToUpper();
            return Instance.ItemValues.FindAll(item => item.ItemId.ToUpper().Contains(itemId) || (item.Alias != null && item.Alias.Find(alias => alias.ToUpper().Contains(itemId)) != null));
        }

        public static BountyItem FindItem(string itemId)
        {
            return Instance.ItemValues.Find(item => item.ItemId.Equals(itemId));
        }

        public static float ItemValue(string itemId)
        {
            BountyItem bi = FindItem(itemId);
            if (bi == null) return 0;
            return bi.Value;
        }

        public static List<Block> FindBlockFuzzy(string itemId)
        {
            itemId = itemId.ToUpper();
            return Instance.BlockValues.FindAll(item => item.BlockId.ToUpper().Contains(itemId) || (item.Alias != null && item.Alias.Find(alias => alias.ToUpper().Contains(itemId)) != null));
        }

        public static Block FindBlock(string itemId)
        {
            return Instance.BlockValues.Find(item => item.BlockId.Equals(itemId));
        }

        public static float BlockValue(string itemId)
        {
            Block bl = FindBlock(itemId);
            if (bl == null) return 0;
            return bl.Value;
        }

        public static void CalculateOres()
        {
            foreach(BountyItem bi in Instance.ItemValues)
            {
                if (!bi.ItemId.StartsWith("MyObjectBuilder_Ore") || bi.Components == null || bi.Components.Count == 0) continue;
                bi.Value = bi.Components[0].Value * ItemValue(bi.Components[0].ItemId);
            }
        }

        public static void CalculateComponent()
        {
            foreach (BountyItem bi in Instance.ItemValues)
            {
                if (bi.ItemId.StartsWith("MyObjectBuilder_Ore") || bi.ItemId.StartsWith("MyObjectBuilder_Ingot") || bi.Components == null) continue;
                bi.Value = 0;
                foreach(Item comp in bi.Components)
                {
                    bi.Value += ItemValue(bi.Components[0].ItemId) * comp.Value / MyAPIGateway.Session.SessionSettings.AssemblerEfficiencyMultiplier;
                }
            }
        }

        public static void CalculateBlocks()
        {
            foreach (Block bl in Instance.BlockValues)
            {
                if (bl.Components == null) continue;
                bl.Value = 0;
                foreach (Item comp in bl.Components)
                {
                    BountyItem valueComponent = FindItem(comp.ItemId);
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
                Logging.Instance.WriteLine("Got Value of " + block.FatBlock.BlockDefinition.ToString());

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
                        Logging.Instance.WriteLine("Item " + v.Value.Type.ToString());
                        cargoValue += (float)v.Value.Amount * ItemValue(v.Value.Type.ToString());
                    }
                }
            }
            return new PointValue() { CargoValue = cargoValue, GridValue = gridValue };
        }

        public struct PointValue
        {
            public double CargoValue;
            public double GridValue;
        }
    }
}
