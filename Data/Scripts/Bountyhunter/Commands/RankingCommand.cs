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

        public override string ArgumentDescription()
        {
            return "<player/faction> <kills/deaths/kdratio/damageDone/damageReceived/damageRatio/bountyPlaced/bountyReceived/bountyClaimed/bounty>";
        }

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
            List<Participant> list = new List<Participant>();
            foreach (Faction f in Participants.Factions.Values) list.Add(f);
            SowRanking(player, v, list, "Faction Ranking for ");
        }

        private void ShowPlayer(IMyPlayer player, string v)
        {
            List<Participant> list = new List<Participant>();
            foreach (Hunter h in Participants.Players.Values) list.Add(h);
            SowRanking(player, v, list, "Player Ranking for ");
        }

        private void SowRanking(IMyPlayer player, string v, List<Participant> list, String title)
        {
            int index = 1;
            StringBuilder content = new StringBuilder();
            switch (v)
            {
                case "kills":
                    list.OrderBy(p => p.Kills);
                    title += "Kills";
                    foreach (Participant p in list)
                    {
                        AppendLine(content, index++, p.ToString(), p.Kills.ToString());
                        if (index >= 999) break;
                    }
                    break;
                case "deaths":
                    list.OrderBy(p => p.Deaths);
                    title += "Deaths";
                    foreach (Participant p in list)
                    {
                        AppendLine(content, index++, p.ToString(), p.Deaths.ToString());
                        if (index >= 999) break;
                    }
                    break;
                case "kdratio":
                    list.OrderBy(p => p.KillDeathRatio);
                    title += "KD-Ratio";
                    foreach (Participant p in list)
                    {
                        AppendLine(content, index++, p.ToString(), p.KillDeathRatio.ToString("0.00"));
                        if (index >= 999) break;
                    }
                    break;
                case "damageDone":
                    list.OrderBy(p => p.DamageDone);
                    title += "Damage done";
                    foreach (Participant p in list)
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.DamageDone));
                        if (index >= 999) break;
                    }
                    break;
                case "damageReceived":
                    list.OrderBy(p => p.DamageReceived);
                    title += "Damage received";
                    foreach (Participant p in list)
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.DamageReceived));
                        if (index >= 999) break;
                    }
                    break;
                case "damageRatio":
                    list.OrderBy(p => p.DamageRatio);
                    title += "Damageratio";
                    foreach (Participant p in list)
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.DamageRatio));
                        if (index >= 999) break;
                    }
                    break;
                case "bountyPlaced":
                    list.OrderBy(p => p.BountyPlaced);
                    title += "Bounty placed";
                    foreach (Participant p in list)
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.BountyPlaced));
                        if (index >= 999) break;
                    }
                    break;
                case "bountyReceived":
                    list.OrderBy(p => p.BountyReceived);
                    title += "Bounty received";
                    foreach (Participant p in list)
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.BountyReceived));
                        if (index >= 999) break;
                    }
                    break;
                case "bountyClaimed":
                    list.OrderBy(p => p.BountyClaimed);
                    title += "Bounty claimed";
                    foreach (Participant p in list)
                    {
                        AppendLine(content, index++, p.ToString(), Formater.FormatCurrency(p.BountyClaimed));
                        if (index >= 999) break;
                    }
                    break;
                case "bounty":
                    list.OrderBy(p => p.BountyWorth);
                    title += "current Bounty";
                    foreach (Participant p in list)
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
            builder.Append((index++).ToString("000"));
            builder.Append(" - ");
            builder.Append(name);
            builder.Append(" -> ");
            builder.Append(value);
            builder.Append("\n");
        }
    }
}
