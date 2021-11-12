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
    class BanCommand : AbstactCommandHandler
    {

        public BanCommand() : base("ban", MyPromoteLevel.Moderator) {

        }

        public override string ArgumentDescription => "<player> <time> <m/h/d> [reason]\nSuspends a players ability to claim bounties for the given time. " +
            "If time is 0 an existing ban will be lifted. m = Minutes, h = hours, d = days.";

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if(arguments.Length < 3)
            {
                WrongArguments(player);
                return;
            }

            long time;
            if (!long.TryParse(arguments[1], out time))
            {
                WrongArguments(player);
                return;
            }

            List<IMyIdentity> list = Utilities.GetPlayerIdentityFuzzy(arguments[0]);
            StringBuilder builder = new StringBuilder();
            if (list.Count != 1)
            {
                builder.Append("No player with that name found.");
                if (list.Count > 1)
                {
                    builder.Append(" Did you mean...\n");
                    foreach (IMyIdentity id in list)
                    {
                        builder.Append("- ");
                        builder.Append(id.DisplayName);
                        builder.Append("\n");
                    }
                }
                SendMessage(player, builder.ToString());
                return;
            }
            IMyIdentity p = list.FirstOrDefault();

            Hunter hunter = Participants.GetPlayer(p);

            IMyPlayer bannedPlayer = Utilities.GetPlayer(hunter.Name);

            if (time == 0)
            {
                hunter.BannedUntil = DateTime.Now;
                hunter.Banned = false;
                SendMessage(player, "Lifted ban for " + hunter.Name + ".");

                if(bannedPlayer != null)
                    SendMessage(bannedPlayer, "Your suspension has been removed.");
            } else {
                switch(arguments[2])
                {
                    case "m": hunter.BannedUntil = DateTime.Now.AddMinutes(time); break;
                    case "h": hunter.BannedUntil = DateTime.Now.AddHours(time); break;
                    case "d": hunter.BannedUntil = DateTime.Now.AddDays(time); break;
                }
                hunter.Banned = true;

                if (arguments.Length > 3)
                {
                    string reason = "";
                    for(int i = 3; i < arguments.Length; i++)
                    {
                        if (reason.Length > 0) reason += " ";
                        reason += arguments[i];
                    }
                    hunter.BanReason = reason;
                }

                SendMessage(player, "Suspended " + hunter.Name + " from claiming Bounties until " + hunter.BannedUntil.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));

                if (bannedPlayer != null)
                    SendMessage(bannedPlayer, "You have been suspended from claiming any Bounties until " + hunter.BannedUntil.ToString("g", CultureInfo.CreateSpecificCulture("de-DE")));

            }
        }
    }
}
