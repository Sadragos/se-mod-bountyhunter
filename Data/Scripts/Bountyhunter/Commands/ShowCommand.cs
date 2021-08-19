using Bountyhunter.Store;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.ModAPI;

namespace Bountyhunter.Commands
{
    class ShowCommand : AbstactCommandHandler
    {

        public ShowCommand() : base("show") {

        }


        public override string ArgumentDescription => "<player/faction> (name)";

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
            List<IMyIdentity> list = Utilities.GetPlayerIdentityFuzzy(v);
            StringBuilder builder = new StringBuilder();
            if (list.Count != 1)
            {
                builder.Append("No player with that name found.");
                if(list.Count > 1)
                {
                    builder.Append(" Did you mean...\n");
                    foreach(IMyIdentity id in list)
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
            string title = "Player: " + hunter.ToString();
            
            
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
            string title = "Faction: " + faction.ToString();
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
                builder.Append(Formater.PadRight(d.Time, 260));
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
                builder.Append(Formater.PadRight(d.Time, 260));
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
            builder.Append(Formater.PadRight(key, 300));
            builder.Append(value);
            builder.Append("\n");
        }

        private void AddInfo(StringBuilder builder, Participant participant)
        {
            builder.Append(">> STATS\n");
            AddLine(builder, "• Kills", participant.Kills.ToString());
            AddLine(builder, "• Deaths", participant.Deaths.ToString());
            AddLine(builder, "• Kill-Death-Ratio", participant.KillDeathRatio.ToString("0.00"));
            builder.Append("\n");
            AddLine(builder, "• Damage Done", Formater.FormatCurrency(participant.DamageDone));
            AddLine(builder, "• Damage Received", Formater.FormatCurrency(participant.DamageReceived));
            AddLine(builder, "• Damage-Ratio", participant.DamageRatio.ToString("0.00"));
            builder.Append("\n");
            AddLine(builder, "• Bounty Placed", Formater.FormatCurrency(participant.BountyPlaced));
            AddLine(builder, "• Bounty Received", Formater.FormatCurrency(participant.BountyReceived));
            AddLine(builder, "• Bounty Claimed", Formater.FormatCurrency(participant.BountyClaimed));
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
            foreach (Bounty b in participant.Bounties)
            {
                ItemConfig item = Values.Items[b.RewardItem.ItemId];
                builder.Append(Formater.PadRight(b.BountyType.Equals(EBountyType.Kill) ? "KILL" : "DMG", 70));
                builder.Append(" ");
                if (b.BountyType.Equals(EBountyType.Kill))
                {
                    float percentage = b.RewardItem.Remaining / b.RewardItem.Value;
                    int killsRemaining = (int)Math.Round(percentage * b.Count);
                    float perKill = item.Value * b.RewardItem.Value / b.Count;
                    builder.Append(Formater.PadLeft(killsRemaining.ToString(), 100));
                    builder.Append(" / ");
                    builder.Append(Formater.PadRight(b.Count.ToString(), 100));
                } else
                {
                    float percentage = b.RewardItem.Remaining / b.RewardItem.Value;
                    float damageRemaining = percentage * b.Count;
                    builder.Append(Formater.PadLeft(Formater.FormatNumber(damageRemaining), 100));
                    builder.Append(" / ");
                    builder.Append(Formater.PadRight(Formater.FormatNumber(b.Count), 100));

                }
                builder.Append(" -->  [");
                builder.Append(Formater.PadCenter(Formater.FormatCurrency(item.Value * b.RewardItem.Value), 160));
                builder.Append("]  ");
                builder.Append(Formater.FormatNumber(b.RewardItem.Value) + " " + item.ToString());


                builder.Append("\n");
            }
            builder.Append("\n");
        }
    }
}
