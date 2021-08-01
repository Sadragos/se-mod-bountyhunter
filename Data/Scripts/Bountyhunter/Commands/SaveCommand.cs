using Bountyhunter.Store;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace Bountyhunter.Commands
{
    class SaveCommand : AbstactCommandHandler
    {

        public SaveCommand() : base("save", MyPromoteLevel.Admin) {

        }

        public override string ArgumentDescription()
        {
            return "";
        }

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            Values.Save();
            if(player != null) SendMessage(player, "Values saved.");
            Bounties.Save();
            if (player != null) SendMessage(player, "Bounties saved.");
        }
    }
}
