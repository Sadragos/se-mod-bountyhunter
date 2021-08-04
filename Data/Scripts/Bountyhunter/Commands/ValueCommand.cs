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

        public override string ArgumentDescription()
        {
            return "<grid/item/block> [searchtext]";
        }

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if (arguments.Length >= 1 && arguments[0].Equals("grid"))
            {
                foreach (IMyCubeGrid grid in Utilities.GetGridsNearPlayer(player))
                {
                    string name = grid.CustomName;
                    PointValue thread = CalculateValue(grid, true);

                    SendMessage(player, "Value of " + name 
                        + "\n-> Empty " + Formater.FormatCurrency(thread.GridValue)
                        + "\n-> Cargo " + Formater.FormatCurrency(thread.CargoValue)
                        + "\n-> Total " + Formater.FormatCurrency(thread.GridValue + thread.CargoValue));
                }
            }
            else if (arguments.Length >= 2 && arguments[0].Equals("item"))
            {
                if (arguments[1].Length < 2)
                {
                    SendMessage(player, "Please enter atelast two letters to search for.");
                }
                List<BountyItem> items = FindItemFuzzy(arguments[1]);
                if (items.Count == 0)
                {
                    SendMessage(player, "Not Item with " + arguments[1] + " found.");
                }
                else
                {
                    int i = 0;
                    while (i < 10 && i < items.Count)
                    {
                        SendMessage(player, items[i].ToString() + ": " + Formater.FormatCurrency(items[i].Value));
                        i++;
                    }
                    if (i < items.Count)
                    {
                        SendMessage(player, "and " + (items.Count - i - 1) + " more");
                    }
                }
            }
            else if (arguments.Length >= 2 && arguments[0].Equals("block"))
            {
                if (arguments[1].Length < 2)
                {
                    SendMessage(player, "Please enter atelast two letters to search for.");
                }
                List<Block> items = Values.FindBlockFuzzy(arguments[1]);
                if (items.Count == 0)
                {
                    SendMessage(player, "Not Item with " + arguments[1] + " found.");
                }
                else
                {
                    int i = 0;
                    while (i < 10 && i < items.Count)
                    {
                        SendMessage(player, items[i].ToString() + ": " + Formater.FormatCurrency(items[i].Value));
                        i++;
                    }
                    if (i < items.Count)
                    {
                        SendMessage(player, "and " + (items.Count - i - 1) + " more");
                    }
                }
            } else
            {
                WrongArguments(player);
            }
        }
    }
}
