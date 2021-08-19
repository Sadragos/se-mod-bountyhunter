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
    class HelpCommand : AbstactCommandHandler
    {

        public HelpCommand() : base("help") {

        }

        public override string ArgumentDescription()
        {
            return "";
        }

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            StringBuilder builder = new StringBuilder();
            foreach(AbstactCommandHandler handler in Core.CommandHandlers)
            {
                if (!handler.HasRank(player)) continue;
                builder.Append("/bh ");
                builder.Append(handler.CommandPrefix);
                builder.Append(" ");
                builder.Append(handler.ArgumentDescription());
                builder.Append("\n\n");
            }
            Utilities.ShowDialog(player.SteamUserId, "Bountyhunt - Help", builder.ToString());
        }
    }
}