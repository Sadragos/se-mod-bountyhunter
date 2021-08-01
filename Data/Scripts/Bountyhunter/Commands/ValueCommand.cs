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

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if (arguments.Length >= 1 && arguments[0].Equals("grid"))
            {

                BoundingSphereD sphere = new BoundingSphereD(player.GetPosition(), 20);
                foreach (IMyEntity ent in MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere))
                {
                    if (!(ent is IMyCubeGrid)) continue;
                    string name = (ent as IMyCubeGrid).CustomName;
                    PointValue thread = Values.CalculateValue(ent as IMyCubeGrid, true);

                    SendMessage(player, "Value of " + name);
                    SendMessage(player, "Empty " + thread.GridValue.ToString("0.000"));
                    SendMessage(player, "Cargo " + thread.CargoValue.ToString("0.000"));
                    SendMessage(player, "Total " + (thread.GridValue + thread.CargoValue).ToString("0.000"));
                }
            }
            else if (arguments.Length >= 2 && arguments[0].Equals("item"))
            {
                if (arguments[1].Length <= 2)
                {
                    SendMessage(player, "Please enter atelast two letters to search for.");
                }
                List<BountyItem> items = Values.FindItemFuzzy(arguments[1]);
                if (items.Count == 0)
                {
                    SendMessage(player, "Not Item with " + arguments[1] + " found.");
                }
                else
                {
                    int i = 0;
                    while (i < 10 && i < items.Count)
                    {
                        SendMessage(player, items[i].ToString() + ": " + items[i].Value.ToString("0.0000"));
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
                if (arguments[1].Length <= 2)
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
                        SendMessage(player, items[i].ToString() + ": " + items[i].Value.ToString("0.0000"));
                        i++;
                    }
                    if (i < items.Count)
                    {
                        SendMessage(player, "and " + (items.Count - i - 1) + " more");
                    }
                }
            }
        }
    }
}
