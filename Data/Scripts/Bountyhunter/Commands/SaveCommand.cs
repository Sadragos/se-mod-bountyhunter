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

        public override string ArgumentDescription => "\nSaves all pending changes to files.";

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            //Values.Save();
            //if(player != null) SendMessage(player, "Values saved.");
            Participants.Save();
            if (player != null) SendMessage(player, "Participants saved.");
            //Config.Save();
            //if (player != null) SendMessage(player, "Config saved.");
            //Killmessages.Save();
            //if (player != null) SendMessage(player, "Killmessages saved.");
        }
    }
}
