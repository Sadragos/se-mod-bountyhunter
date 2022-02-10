using Bountyhunter.Store;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VRage.Game.ModAPI;

namespace Bountyhunter.Commands
{
    class ResetCommand : AbstactCommandHandler
    {

        public ResetCommand() : base("reset", MyPromoteLevel.Moderator) {

        }

        public override string ArgumentDescription => "!!! <filename>\nAdd the three !!! to really make sure, this is not a mistake. This will reset all Bountyhunter Rankings to 0 while keeping all active bounties active. Filename is optional year-month will be used by default.";

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if(arguments.Length < 1 || !arguments[0].Equals("!!!"))
            {
                WrongArguments(player);
                return;
            }
            SendMessage(player, "Creating backup of Participants.xml ...");
            Participants.Save();
            Participants.Save("Participants.xml.bak");
            DateTime resetTime = DateTime.Now;
            if (Config.Instance.KeepMonthlyHistory)
            {
                string filename = arguments.Length >= 2 ? arguments[1] : resetTime.Year + "-" + resetTime.Month;
                SendMessage(player, "Preparing Data-File ...");
                Participants.Instance.Factions.ForEach(f =>
                {
                    f.Bounties = null;
                });
                Participants.Instance.Players.ForEach(p =>
                {
                    p.Bounties = null;
                    p.DeathList = null;
                    p.KillList = null;
                    p.ClaimableBounty = null;
                });
                Participants.Save("Participants_" + filename + ".xml");
            }

            SendMessage(player, "Resetting Statistics & Cleanup ...");
            Participants.Load();
            Participants.Instance.Players.RemoveAll(p =>
            {
                p.Reset();
                return !p.Relevant();
            });
            Participants.Instance.Factions.RemoveAll(f =>
            {
                f.Reset();
                return !f.Relevant();
            });
            Participants.Instance2Dict();
            Participants.Save();
            Config.Instance.LastReset = resetTime;
            Config.Save();
            SendMessage(player, "Statistics reset complete!");
            Utilities.ShowChatMessage("The Bountyhunter-Statistics have been set to Zero again. Good Hunting!");
        }
    }
}
