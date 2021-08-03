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

        public override string ArgumentDescription()
        {
            return "";
        }

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
