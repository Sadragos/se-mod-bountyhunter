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
            "\n- bounty\n   Which head has currently the highest total bounty on it." +
            "\n- kills\n   Shows who has killed the most players." +
            "\n- deaths\n   Shows who died the most." +
            "\n- kdratio\n   Shows who has the best kill-death-ratio. " +
            "\n- damageDone\n   Shows who has done the most damage to enemy grids." +
            "\n- damageReceived\n   Shows who has received the most damage from enemies." +
            "\n- damageRatio\n   Shows who has the best damagae ratio." +
            "\n- bountyPlaced\n   Shows who has placed the most bounties in total." +
            "\n- bountyReceived\n   Shows, on which had was placed the most bounty in total." +
            "\n- bountyClaimed\n   Shows who has earned the most bounties in total." +
            "\n- killValue\n   Shows which kill is worth the most." +
            "\n- damageValue\n   Shows which damage is worth the most.";

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
            string title = faction ? "Faction Ranking for " : "Player Ranking for ";
            StringBuilder content = new StringBuilder();
            switch (v)
            {
                case "kills":
                    title += "Kills";
                    break;
                case "deaths":
                    title += "Deaths";
                    break;
                case "kdratio":
                    title += "KD-Ratio";
                    break;
                case "damageDone":
                    title += "Damage done";
                    break;
                case "damageReceived":
                    title += "Damage received";
                    break;
                case "damageRatio":
                    title += "Damageratio";
                    break;
                case "bountyPlaced":
                    title += "Bounty placed";
                    break;
                case "bountyReceived":
                    title += "Bounty received";
                    break;
                case "bountyClaimed":
                    title += "Bounty claimed";
                    break;
                case "bounty":
                    title += "current Bounty";
                    break;
                case "killValue":
                    title += "current Kill-Value";
                    break;
                case "damageValue":
                    title += "current Damage-Value";
                    break;
                default:
                    WrongArguments(player);
                    return;
            }
            AppendRanking(content, v, faction, 500, false);
            Utilities.ShowDialog(player.SteamUserId, title, content.ToString());
        }

        public static void AppendRanking(StringBuilder sb, string ranking, bool faction, int limit, bool monospace = false)
        {
            int index = 1;
            switch (ranking)
            {
                case "kills":
                    foreach (Participant p in Rankings.GetRanking(ranking, faction))
                    {
                        AppendLine(sb, index++, p.ToString(), p.Kills.ToString(), monospace);
                        if (index >= limit) break;
                    }
                    break;
                case "deaths":
                    foreach (Participant p in Rankings.GetRanking(ranking, faction))
                    {
                        AppendLine(sb, index++, p.ToString(), p.Deaths.ToString(), monospace);
                        if (index >= limit) break;
                    }
                    break;
                case "kdratio":
                    foreach (Participant p in Rankings.GetRanking(ranking, faction))
                    {
                        AppendLine(sb, index++, p.ToString(), p.KillDeathRatio.ToString("0.00"), monospace);
                        if (index >= limit) break;
                    }
                    break;
                case "damageDone":
                    foreach (Participant p in Rankings.GetRanking(ranking, faction))
                    {
                        AppendLine(sb, index++, p.ToString(), Formater.FormatCurrency(p.DamageDone), monospace);
                        if (index >= limit) break;
                    }
                    break;
                case "damageReceived":
                    foreach (Participant p in Rankings.GetRanking(ranking, faction))
                    {
                        AppendLine(sb, index++, p.ToString(), Formater.FormatCurrency(p.DamageReceived), monospace);
                        if (index >= limit) break;
                    }
                    break;
                case "damageRatio":
                    foreach (Participant p in Rankings.GetRanking(ranking, faction))
                    {
                        AppendLine(sb, index++, p.ToString(), Formater.FormatCurrency(p.DamageRatio), monospace);
                        if (index >= limit) break;
                    }
                    break;
                case "bountyPlaced":
                    foreach (Participant p in Rankings.GetRanking(ranking, faction))
                    {
                        AppendLine(sb, index++, p.ToString(), Formater.FormatCurrency(p.BountyPlaced), monospace);
                        if (index >= limit) break;
                    }
                    break;
                case "bountyReceived":
                    foreach (Participant p in Rankings.GetRanking(ranking, faction))
                    {
                        AppendLine(sb, index++, p.ToString(), Formater.FormatCurrency(p.BountyReceived), monospace);
                        if (index >= limit) break;
                    }
                    break;
                case "bountyClaimed":
                    foreach (Participant p in Rankings.GetRanking(ranking, faction))
                    {
                        AppendLine(sb, index++, p.ToString(), Formater.FormatCurrency(p.BountyClaimed), monospace);
                        if (index >= limit) break;
                    }
                    break;
                case "bounty":
                    foreach (Participant p in Rankings.GetRanking(ranking, faction))
                    {
                        AppendLine(sb, index++, p.ToString(), Formater.FormatCurrency(p.BountyWorth), monospace);
                        if (index >= limit) break;
                    }
                    break;
                case "killValue":
                    foreach (Participant p in Rankings.GetRanking(ranking, faction))
                    {
                        AppendLine(sb, index++, p.ToString(), Formater.FormatCurrency(p.KillValue), monospace);
                        if (index >= limit) break;
                    }
                    break;
                case "damageValue":
                    foreach (Participant p in Rankings.GetRanking(ranking, faction))
                    {
                        AppendLine(sb, index++, p.ToString(), Formater.FormatCurrency(p.DamageValue), monospace);
                        if (index >= limit) break;
                    }
                    break;
            }
        }

        private static void AppendLine(StringBuilder builder, int index, string name, string value, bool monospace = false)
        {
            if (monospace)
            {
                int targetWidth = 20;
                string newName = name;
                if (newName.Length > targetWidth)
                    newName = newName.Substring(0, targetWidth);
                else if (newName.Length < targetWidth)
                    newName += new string(' ', targetWidth - newName.Length);

                builder
                    .Append(index.ToString("000"))
                    .Append(" | ")
                    .Append(newName)
                    .Append(" | ")
                    .Append(value)
                    .Append("\n");
            }
            else
            {
                builder.Append(Formater.PadRight(index.ToString("000"), 60))
                    .Append("  |  ")
                    .Append(Formater.PadRight(name, 700))
                    .Append("  |  ")
                    .Append(value)
                    .Append("\n");
            }
        }
    }
}
