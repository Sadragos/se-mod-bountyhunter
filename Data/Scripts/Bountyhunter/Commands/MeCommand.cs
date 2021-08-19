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
    class MeCommand : AbstactCommandHandler
    {

        public MeCommand() : base("me") {

        }

        public override string ArgumentDescription => "";

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            new ShowCommand().HandleCommand(player, new string[] { "player", player.DisplayName });
        }
    }
}
