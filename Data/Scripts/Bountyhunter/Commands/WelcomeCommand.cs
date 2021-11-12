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
    class WelcomeCommand : AbstactCommandHandler
    {

        public WelcomeCommand() : base("welcome") {

        }

        public override string ArgumentDescription => "\nShows you a short Message with your current bounty status.";

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            Hunter hunter = Participants.GetPlayer(player.Identity);
            if(hunter == null)
                Utilities.ShowChatMessage("Welcome to Bountyhunt. You're currently a new Player on the board.", player.IdentityId);
            else if (hunter.BountyWorth == 0)
                Utilities.ShowChatMessage("Welcome to Bountyhunt. You currently don't have any Bounty on you or your Faction.", player.IdentityId);
            else
                Utilities.ShowChatMessage("Welcome to Bountyhunt. Caution, you currently have " + Formater.FormatCurrency(hunter.BountyWorth) + " Bounty on you!", player.IdentityId);
        }
    }
}
