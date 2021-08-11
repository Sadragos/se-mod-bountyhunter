using Bountyhunter.Store;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace Bountyhunter.Commands
{
    class ShowCommand : AbstactCommandHandler
    {

        public ShowCommand() : base("show") {

        }

        public override string ArgumentDescription()
        {
            return "<player/faction> (name)";
        }

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if (arguments.Length != 2)
            {
                WrongArguments(player);
                return;
            }

            if (arguments[0].Equals("faction") || arguments[0].Equals("f")) ShowFaction(player, arguments[1]);
            else if (arguments[0].Equals("player") || arguments[0].Equals("p")) ShowPlayer(player, arguments[1]);
            else WrongArguments(player);
        }

        private void ShowPlayer(IMyPlayer player, string v)
        {
            IMyIdentity p = Utilities.GetPlayerIdentity(v);
            if(p == null)
            {
                SendMessage(player, "No player with that name found");
                return;
            }

            Hunter hunter = Participants.GetPlayer(p);
            string title = "Playerdetails: " + hunter.Name;
            StringBuilder builder = new StringBuilder();
            AddLine(builder, "Faction", hunter.FactionTag);
            AddInfo(builder, hunter);

            AddBounties(builder, hunter);
            AddKills(builder, hunter);
            AddDeaths(builder, hunter);

            Utilities.ShowDialog(player.SteamUserId, title, builder.ToString());
        }

        private void ShowFaction(IMyPlayer player, string v)
        {
            IMyFaction f = Utilities.GetFactionByTag(v);
            if (f == null)
            {
                SendMessage(player, "No faction with that name found");
                return;
            }

            Faction faction = Participants.GetFaction(f);
            string title = "Factiondetails: " + faction.Name;
            StringBuilder builder = new StringBuilder();
            builder.Append(">> MEMBERS (");
            builder.Append(faction.Members.Count);
            builder.Append(")\n");
            builder.Append(string.Join(", ", faction.Members));
            builder.Append("\n\n");

            AddInfo(builder, faction);

            AddBounties(builder, faction);

            // TODO Kill/Death stats
            //AddKills(builder, faction);
            //AddDeaths(builder, faction);


            Utilities.ShowDialog(player.SteamUserId, title, builder.ToString());
        }

        private void AddDeaths(StringBuilder builder, Hunter particiant)
        {
            if (particiant.GetKillList().Count == 0)
            {
                builder.Append(">> NO KILLS FOUND\n\n");
                return;
            }
            builder.Append(">> KILLS\n");
            foreach(Death d in particiant.GetKillList())
            {
                builder.Append("- ");
                builder.Append(d.Time);
                builder.Append(" -> ");
                if (!string.IsNullOrEmpty(d.Opponent))
                {
                    builder.Append(d.Opponent);
                    builder.Append(" ");
                }
                builder.Append("by ");
                builder.Append(d.Reason);
                if(d.ClaimedBounty > 0)
                {
                    builder.Append(" for ");
                    builder.Append(Formater.FormatCurrency(d.ClaimedBounty));
                }
                builder.Append("\n");
            }
            builder.Append("\n");
        }

        private void AddKills(StringBuilder builder, Hunter particiant)
        {
            if (particiant.GetDeathList().Count == 0)
            {
                builder.Append(">> NO DEATHS FOUND\n\n");
                return;
            }
            builder.Append(">> DEATHS\n");
            foreach (Death d in particiant.GetDeathList())
            {
                builder.Append("- ");
                builder.Append(d.Time);
                builder.Append(" -> ");
                if(!string.IsNullOrEmpty(d.Opponent))
                {
                    builder.Append(d.Opponent);
                    builder.Append(" ");
                }
                builder.Append("by ");
                builder.Append(d.Reason);
                if (d.ClaimedBounty > 0)
                {
                    builder.Append(" for ");
                    builder.Append(Formater.FormatCurrency(d.ClaimedBounty));
                }
                builder.Append("\n");
            }
            builder.Append("\n");
        }

        private void AddLine(StringBuilder builder, string key, string value)
        {
            builder.Append(key);
            builder.Append(": ");
            builder.Append(value);
            builder.Append("\n");
        }

        private void AddInfo(StringBuilder builder, Participant participant)
        {
            builder.Append(">> STATS\n");
            AddLine(builder, "Kills", participant.Kills.ToString());
            AddLine(builder, "Deaths", participant.Deaths.ToString());
            AddLine(builder, "Kill-Death-Ratio", participant.KillDeathRatio.ToString());
            builder.Append("\n");
            AddLine(builder, "Damage Done", Formater.FormatCurrency(participant.DamageDone));
            AddLine(builder, "Damage Received", Formater.FormatCurrency(participant.DamageReceived));
            AddLine(builder, "Damage-Ratio", Formater.FormatCurrency(participant.DamageRatio));
            builder.Append("\n");
            AddLine(builder, "Bounty Placed", Formater.FormatCurrency(participant.BountyPlaced));
            AddLine(builder, "Bounty Received", Formater.FormatCurrency(participant.BountyReceived));
            AddLine(builder, "Bounty Claimed", Formater.FormatCurrency(participant.BountyClaimed));
            builder.Append("\n");
        }

        private void AddBounties(StringBuilder builder, Participant participant)
        {
            if(participant.Bounties.Count == 0)
            {
                builder.Append(">> NO BOUNTIES FOUND\n\n");
                return;
            }
            builder.Append(">> BOUNTIES (");
            builder.Append(Formater.FormatCurrency(participant.BountyWorth));
            builder.Append(")\n");
            foreach(Bounty b in participant.Bounties)
            {
                ItemConfig item = Values.Items[b.RewardItem.ItemId];
                builder.Append("- ");
                builder.Append(b.BountyType.Equals(EBountyType.Kill) ? "KIL" : "DMG");
                builder.Append(" ");
                if (b.BountyType.Equals(EBountyType.Kill))
                {
                    builder.Append(b.Count);
                    builder.Append("x for ");
                } else
                {
                    builder.Append("for ");
                    builder.Append(Formater.FormatCurrency(b.Count));
                    builder.Append(" -> ");
                }
                builder.Append(Formater.FormatNumber(b.RewardItem.Value));
                builder.Append(" ");
                builder.Append(item.ToString());
                builder.Append(" (");
                builder.Append(Formater.FormatCurrency(item.Value * b.RewardItem.Value));
                builder.Append(") total. ");
                if (b.BountyType.Equals(EBountyType.Kill))
                {
                    float percentage = b.RewardItem.Remaining / b.RewardItem.Value;
                    int killsRemaining = (int)Math.Round(percentage * b.Count);
                    float perKill = item.Value * b.RewardItem.Value / b.Count;
                    builder.Append(killsRemaining);
                    builder.Append(" kills remaining.");
                }
                else
                {
                    float percentage = b.RewardItem.Remaining / b.RewardItem.Value;
                    float damageRemaining = percentage * b.Count;
                    builder.Append(Formater.FormatCurrency(damageRemaining));
                    builder.Append(" damage remaining.");
                }

                builder.Append("\n");
            }
            builder.Append("\n");
        }
    }
}
