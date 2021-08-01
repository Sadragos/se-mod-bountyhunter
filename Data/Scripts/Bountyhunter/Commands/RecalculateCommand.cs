using Bountyhunter.Store;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace Bountyhunter.Commands
{
    class RecalculateCommand : AbstactCommandHandler
    {

        public RecalculateCommand() : base("recalculate", MyPromoteLevel.Admin) {

        }

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if(arguments.Length == 1)
            {
                if(arguments[0].Equals("ores"))
                {
                    RecalculateOres(player);
                } else if (arguments[0].Equals("components"))
                {
                    RecalculateComponents(player);
                } else if (arguments[0].Equals("blocks"))
                {
                    RecalculateBlocks(player);
                }
            }
            else
            {
                WrongArguments(player);
                return;
            }
        }

        private void RecalculateBlocks(IMyPlayer player)
        {
            Values.CalculateBlocks();
            SendMessage(player, "Blocks recalculated. Use /bh save to store changes.");
        }

        private void RecalculateComponents(IMyPlayer player)
        {
            Values.CalculateComponent();
            SendMessage(player, "Craftables recalculated. Use /bh save to store changes.");
        }

        private void RecalculateOres(IMyPlayer player)
        {
            Values.CalculateOres();
            SendMessage(player, "Ores recalculated. Use /bh save to store changes.");
        }

        private void WrongArguments(IMyPlayer player)
        {
            SendMessage(player, "Wrong Arguments");
            SendMessage(player, CommandPrefix + " recalculate <ores/components/blocks>");
        }
    }
}
