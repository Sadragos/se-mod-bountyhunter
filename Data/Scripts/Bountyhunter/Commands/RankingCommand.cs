using Bountyhunter.Store;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace Bountyhunter.Commands
{
    class RankingCommand : AbstactCommandHandler
    {

        public RankingCommand() : base("rank") {

        }

        public override string ArgumentDescription()
        {
            return "<player/faction> <kills/deaths/ratio/damageDone/damageReceived/bountyPlaced/bountyClaimed/bounty>";
        }

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if(arguments.Length != 2)
            {
                WrongArguments(player);
                return;
            }

            if (arguments[0].Equals("faction") || arguments[0].Equals("f")) ShowFaction(arguments[1]);
            else if (arguments[0].Equals("player") || arguments[0].Equals("p")) ShowPlayer(arguments[1]);
            else WrongArguments(player);
        }

        private void ShowFaction(string v)
        {
            
        }

        private void ShowPlayer(string v)
        {
            
        }
    }
}
