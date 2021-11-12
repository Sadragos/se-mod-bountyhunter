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
using Bountyhunter.Utils;
using Bountyhunter.Store;
using static Bountyhunter.Store.Values;
using Bountyhunter.Store.Proto;

namespace Bountyhunter.Commands
{
    class ValueCommand : AbstactCommandHandler
    {

        public ValueCommand() : base("value") {

        }

        public override string ArgumentDescription => "<grid/item/block> [searchtext]\nEvery Item and Block is assigned a value, which is most relevant for damage bounties and to display a unified bounty value. This command shows you the value if a grid, an item or a block. Searchtext is not required for grids. There are some things to keep in mind:" +
            "\n- grid\n   Shows you the value of grids around you in a small radius. You have to own enough Blocks on the grids." +
            "\n- item\n   Doubles as an item search, which means you can enter only a few letters of the name.Every item also gets a uique ID which can be used when placing bounties." +
            "\n- block\n   Doubles as a block search, which means you can enter only a few letters of the name.";

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if (arguments.Length >= 1 && arguments[0].Equals("grid"))
            {
                foreach (IMyCubeGrid grid in Utilities.GetGridsNearPlayer(player))
                {
                    string name = grid.CustomName;
                    
                    PointValue thread = CalculateValue(grid, true);

                    float percentage = thread.GetOwnedPercent(player.IdentityId);
                    if (percentage >= Config.Instance.GridValueMinOwnershipPercent)
                    {
                        SendMessage(player, "Value of " + name
                            + "\n-> Empty " + Formater.FormatCurrency(thread.GridValue)
                            + "\n-> Cargo " + Formater.FormatCurrency(thread.CargoValue)
                            + "\n-> Total " + Formater.FormatCurrency(thread.GridValue + thread.CargoValue));
                    } else
                    {
                        SendMessage(player, "Value of " + name + " could not be determined as you don't own enough of it.");
                    }
                }
            }
            else if (arguments.Length >= 2 && arguments[0].Equals("item"))
            {
                if (arguments[1].Length < 2)
                {
                    SendMessage(player, "Please enter atleast two letters to search for.");
                }
                List<ItemConfig> items = FindItemFuzzy(arguments[1]);
                if (items.Count == 0)
                {
                    SendMessage(player, "No Item with " + arguments[1] + " found.");
                }
                else
                {
                    items.OrderBy(i => i.ToString());
                    string title = "Itemsearch";
                    StringBuilder builder = new StringBuilder();
                    foreach(ItemConfig item in items)
                    {
                        builder.Append(Formater.PadRight(item.BountyId, 120));
                        builder.Append("  |  ");
                        builder.Append(item.ToString());
                        builder.Append(" - ");
                        builder.Append(Formater.FormatCurrency(item.Value));
                        builder.Append("\n");
                    }
                    Utilities.ShowDialog(player.SteamUserId, title, builder.ToString());
                }
            }
            else if (arguments.Length >= 2 && arguments[0].Equals("block"))
            {
                if (arguments[1].Length < 2)
                {
                    SendMessage(player, "Please enter atleast two letters to search for.");
                }
                List<BlockConfig> items = Values.FindBlockFuzzy(arguments[1]);
                if (items.Count == 0)
                {
                    SendMessage(player, "No Block with " + arguments[1] + " found.");
                }
                else
                {
                    items.OrderBy(i => i.ToString());
                    string title = "Blocksearch";
                    StringBuilder builder = new StringBuilder();
                    foreach (BlockConfig item in items)
                    {
                        builder.Append(Formater.PadRight(item.BountyId, 125));
                        builder.Append("  |  ");
                        builder.Append(item.ToString());
                        builder.Append(" - ");
                        builder.Append(Formater.FormatCurrency(item.Value));
                        builder.Append("\n");
                    }
                    Utilities.ShowDialog(player.SteamUserId, title, builder.ToString());
                }
            } else
            {
                WrongArguments(player);
            }
        }
    }
}
