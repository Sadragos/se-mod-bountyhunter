using Bountyhunter.Store;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace Bountyhunter.Commands
{
    class ReloadCommand : AbstactCommandHandler
    {

        public ReloadCommand() : base("reload", MyPromoteLevel.Admin) {

        }

        public override string ArgumentDescription => "\nThis wil reload all relevant XML files from the Disk. This is usefull if you hgave changed them with an external Editor. Make sure to use this command quickly after making your changes, as the file is saved everytime your world is saved, which can result in the loss of your changes.";

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            Config.Load();
            if (player != null) SendMessage(player, "Config loaded.");
            Values.Load();
            if(player != null) SendMessage(player, "Values loaded.");
            Participants.Load();
            if (player != null) SendMessage(player, "Participants loaded.");
        }
    }
}
