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
                if (arguments[0].Equals("ores"))
                {
                    RecalculateOres(player);
                } else if (arguments[0].Equals("components"))
                {
                    RecalculateComponents(player);
                } else if (arguments[0].Equals("blocks"))
                {
                    RecalculateBlocks(player);
                } else if (arguments[0].Equals("all")) {
                    RecalculateOres(player);
                    RecalculateComponents(player);
                    RecalculateBlocks(player);
                } else
                {
                    WrongArguments(player);
                }
            }
            else
            {
                WrongArguments(player);
                return;
            }
        }

        public override string ArgumentDescription => "<all/ores/components/blocks>\nThis will recalculate the values of the given category. Caution: It will override all manual changes to the values in that category.";

        private void RecalculateBlocks(IMyPlayer player)
        {
            Values.CalculateBlocks();
            SendMessage(player, "Blocks recalculated.");
            Values.Save();
        }

        private void RecalculateComponents(IMyPlayer player)
        {
            Values.CalculateComponent();
            SendMessage(player, "Craftables recalculated.");
            Values.Save();
        }

        private void RecalculateOres(IMyPlayer player)
        {
            Values.CalculateOres();
            SendMessage(player, "Ores recalculated.");
            Values.Save();
        }
    }
}
