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
    class ClaimableCommand : AbstactCommandHandler
    {

        public ClaimableCommand() : base("claimable") {

        }

        public override string ArgumentDescription => "\nShows you a list of all bounties, that you've already earned and that can be claimed with the '/bh claim' command.";

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            StringBuilder builder = new StringBuilder();
            Hunter hunter = Participants.GetPlayer(player.Identity);
            foreach(Item item in hunter.ClaimableBounty)
            {
                builder.Append(Formater.PadRight(Formater.FormatNumber(item.Value), 160));
                builder.Append("  ");
                builder.Append(Items[item.ItemId].ToString());
                builder.Append("\n");
            }

            Utilities.ShowDialog(player.SteamUserId, "Claimable Bounties", builder.ToString());
        }
    }
}
