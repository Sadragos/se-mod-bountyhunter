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

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            Values.Load();
            SendMessage(player, "Values loaded.");
        }
    }
}
