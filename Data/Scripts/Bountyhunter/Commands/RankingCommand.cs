using Bountyhunter.Store;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.ModAPI;

namespace Bountyhunter.Commands
{
    class RankingCommand : AbstactCommandHandler
    {

        public RankingCommand() : base("rank") {

        }

        public override string ArgumentDescription => "<player/faction> <ranking>" +
            "\nShows a ranking for the given category. " +
            "You can use 'p' or 'f' instead of player or faction as the first argument. Use one of the following categories as ranking:" +
            "\n- bounty\n   Which head has currently the highest bounty on it." +
            "\n- kills\n   Shows who has killed the most players." +
            "\n- deaths\n   Shows who died the most." +
            "\n- kdratio\n   Shows who has the best kill-death-ratio. " +
            "\n- damageDone\n   Shows who has done the most damage to enemy grids." +
            "\n- damageReceived\n   Shows who has received the most damage from enemies." +
            "\n- damageRatio\n   Shows who has the best damagae ratio." +
            "\n- bountyPlaced\n   Shows who has placed the most bounties in total." +
            "\n- bountyReceived\n   Shows, on which had was placed the most bounty in total." +
            "\n- bountyClaimed\n   Shows who has earned the most bounties in total.";

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if(arguments.Length != 2)
            {
                WrongArguments(player);
                return;
            }

            if (arguments[0].Equals("faction") || arguments[0].Equals("f")) ShowFaction(player, arguments[1]);
            else if (arguments[0].Equals("player") || arguments[0].Equals("p")) ShowPlayer(player, arguments[1]);
            else WrongArguments(player);
        }

        private void ShowFaction(IMyPlayer player, string v)
        {
            SowRanking(player, v,  true);
        }

        private void ShowPlayer(IMyPlayer player, string v)
        {
            SowRanking(player, v,  false);
        }

        private void SowRanking(IMyPlayer player, string v, bool faction)
        {
            int index = 1;
            string title = faction ? "Faction Ranking for " : "Player Ranking for ";
            StringBuilder content = new StringBuilder();
            switch (v)
            {
                case "kills":
                    title += "Kills";
                    foreach (Participant p in Rankings.GetRanking(v, faction))
                    {
                        AppendLine(content, index++, p.ToString(), p.Kills.ToString());
                        if (index >= 999) break;
                    }
                    break;
                case "deaths":
                    title += "Deaths";
                    foreach (Participant p in Rankings.GetRanking(v, faction))
                    {
                        AppendLine(content, index++, p.ToString(), p.Deaths.ToString());
                        if (index >= 999) break;
                    }
                    break;
                case "kdratio":
                    title += "KD-Ratio";
                    foreach (Participant p in Rankings.GetRanking(v, faction))
                    {
                        AppendLine(content, index++, p.ToString(), p.KillDeathRatio.ToString("0.00"));
                        if (index >= 999) break;
                    }
                    break;
                case "damageDone":
                    title += "Damage done";
                    foreach (Participant p in Rankings.GetRanking(v, faction))
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.DamageDone));
                        if (index >= 999) break;
                    }
                    break;
                case "damageReceived":
                    title += "Damage received";
                    foreach (Participant p in Rankings.GetRanking(v, faction))
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.DamageReceived));
                        if (index >= 999) break;
                    }
                    break;
                case "damageRatio":
                    title += "Damageratio";
                    foreach (Participant p in Rankings.GetRanking(v, faction))
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.DamageRatio));
                        if (index >= 999) break;
                    }
                    break;
                case "bountyPlaced":
                    title += "Bounty placed";
                    foreach (Participant p in Rankings.GetRanking(v, faction))
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.BountyPlaced));
                        if (index >= 999) break;
                    }
                    break;
                case "bountyReceived":
                    title += "Bounty received";
                    foreach (Participant p in Rankings.GetRanking(v, faction))
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.BountyReceived));
                        if (index >= 999) break;
                    }
                    break;
                case "bountyClaimed":
                    title += "Bounty claimed";
                    foreach (Participant p in Rankings.GetRanking(v, faction))
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.BountyClaimed));
                        if (index >= 999) break;
                    }
                    break;
                case "bounty":
                    title += "current Bounty";
                    foreach (Participant p in Rankings.GetRanking(v, faction))
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.BountyWorth));
                        if (index >= 999) break;
                    }
                    break;
                default:
                    WrongArguments(player);
                    return;
            }
            Utilities.ShowDialog(player.SteamUserId, title, content.ToString());
        }

        private void AppendLine(StringBuilder builder, int index, string name, string value)
        {
            builder.Append(Formater.PadRight((index++).ToString("000"), 60));
            builder.Append("  |  ");
            builder.Append(Formater.PadRight(name, 700));
            builder.Append("  |  ");
            builder.Append(value);
            builder.Append("\n");
        }
    }
}
