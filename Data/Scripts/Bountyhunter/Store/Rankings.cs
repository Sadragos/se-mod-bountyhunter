using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bountyhunter.Store
{
    class Rankings
    {
        private static Dictionary<string, Ranking> PlayerRankings = new Dictionary<string, Ranking>();
        private static Dictionary<string, Ranking> FactionRankings = new Dictionary<string, Ranking>();

        public static List<Participant> GetPlayerRanking(string ranking)
        {
            return GetRanking(ranking, false);
        }

        public static List<Participant> GetFactionRanking(string ranking)
        {
            return GetRanking(ranking, true);
        }

        public static int RankOfFaction(string ranking, Participant participant)
        {
            return Rank(ranking, participant, true);
        }

        public static int RankOfPlayer(string ranking, Participant participant)
        {
            return Rank(ranking, participant, false);
        }

        public static int Rank(string ranking, Participant participant, bool faction)
        {
            return GetRanking(ranking, faction).FindIndex(p => p.Id.Equals(participant.Id)) + 1;
        }

        public static List<Participant> GetRanking(string ranking, bool faction)
        {
            Dictionary<string, Ranking> target = faction ? FactionRankings : PlayerRankings;
            Ranking rank = null;
            if(!target.TryGetValue(ranking, out rank))
            {
                rank = new Ranking();
                target.Add(ranking, rank);
            }
            if(rank.LastCheck < DateTime.Now.AddSeconds(-Config.Instance.RankingRefreshSeconds))
            {
                rank.Participants.Clear();
                rank.LastCheck = DateTime.Now;
                if (faction) foreach (Faction f in Participants.Factions.Values) rank.Participants.Add(f);
                else foreach (Hunter h in Participants.Players.Values) rank.Participants.Add(h);

                switch (ranking)
                {
                    case "kills":
                        rank.Participants = rank.Participants.OrderByDescending(p => p.Kills).ToList();
                        break;
                    case "deaths":
                        rank.Participants = rank.Participants.OrderByDescending(p => p.Deaths).ToList();
                        break;
                    case "kdratio":
                        rank.Participants = rank.Participants.OrderByDescending(p => p.KillDeathRatio).ToList();
                        break;
                    case "damageDone":
                        rank.Participants = rank.Participants.OrderByDescending(p => p.DamageDone).ToList();
                        break;
                    case "damageReceived":
                        rank.Participants = rank.Participants.OrderByDescending(p => p.DamageReceived).ToList();
                        break;
                    case "damageRatio":
                        rank.Participants = rank.Participants.OrderByDescending(p => p.DamageRatio).ToList();
                        break;
                    case "bountyPlaced":
                        rank.Participants = rank.Participants.OrderByDescending(p => p.BountyPlaced).ToList();
                        break;
                    case "bountyReceived":
                        rank.Participants = rank.Participants.OrderByDescending(p => p.BountyReceived).ToList();
                        break;
                    case "bountyClaimed":
                        rank.Participants = rank.Participants.OrderByDescending(p => p.BountyClaimed).ToList();
                        break;
                    case "bounty":
                        rank.Participants = rank.Participants.OrderByDescending(p => p.BountyWorth).ToList();
                        break;
                }

            }
            return rank.Participants;
        }
    }

    class Ranking
    {
        public List<Participant> Participants = new List<Participant>();
        public DateTime LastCheck = DateTime.Now.AddDays(-100);
    }
}
