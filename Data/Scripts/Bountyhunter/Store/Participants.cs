using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using System.Xml.Serialization;
using static Sandbox.Definitions.MyCubeBlockDefinition;
using ProtoBuf;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using Bountyhunter.Store.Proto.Files;
using VRage.Game;

namespace Bountyhunter.Store
{
    public class Participants
    {

        public static FileParticipants Instance;

        public static Dictionary<long, Hunter> Players = new Dictionary<long, Hunter>();
        public static Dictionary<long, Faction> Factions = new Dictionary<long, Faction>();
        static List<long> Removes = new List<long>();


        public static void Load()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage("Participants.xml", typeof(FileParticipants)))
            {
                try
                {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("Participants.xml", typeof(FileParticipants));
                    var xmlData = reader.ReadToEnd();
                    Instance = MyAPIGateway.Utilities.SerializeFromXML<FileParticipants>(xmlData);
                    reader.Dispose();
                    Logging.Instance.WriteLine("Participants found and loaded");
                }
                catch (Exception e)
                {
                    Logging.Instance.WriteLine("Participants loading failed");
                }
            }

            ValidateData();
            Instance2Dict();
            Save();
        }

        private static void ValidateData()
        {
            if (Instance == null) Instance = new FileParticipants();
        }

        public static void Save()
        {
            try
            {
                Dict2Instance();
                Logging.Instance.WriteLine("Serializing Participants to XML... ");
                string xml = MyAPIGateway.Utilities.SerializeToXML(Instance);
                Logging.Instance.WriteLine("Writing Participants to disk... ");
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("Participants.xml", typeof(FileParticipants));
                writer.Write(xml);
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine("Error saving Participants XML!" + e.StackTrace);
            }
        }

        private static void Dict2Instance()
        {
            Instance.Factions = Factions.Values.ToList();
            Instance.Players = Players.Values.ToList();
        }

        private static void Instance2Dict()
        {
            Factions.Clear();
            foreach (Faction f in Instance.Factions)
            {
                if (Factions.ContainsKey(f.Id))
                {
                    Logging.Instance.WriteLine("WARNING Duplicate Faction Tag " + f.FactionTag);
                    continue;
                }
                Factions.Add(f.Id, f);
            }

            Players.Clear();
            foreach (Hunter p in Instance.Players)
            {
                if (Players.ContainsKey(p.Id))
                {
                    Logging.Instance.WriteLine("WARNING Duplicate Playername " + p.Name);
                    continue;
                }
                Players.Add(p.Id, p);
            }
        }

        public static Hunter GetPlayer(IMyIdentity player, bool create = true)
        {
            Hunter hunter;
            if (!Players.TryGetValue(player.IdentityId, out hunter))
            {
                hunter = new Hunter()
                {
                    Id = player.IdentityId,
                    Name = player.DisplayName
                };
                if (create) Players.Add(player.IdentityId, hunter);
            }
            return hunter;
        }

        public static Hunter GetPlayer(string player, bool create = true)
        {
            return GetPlayer(Utilities.GetPlayerIdentity(player), create);
        }

        public static Faction GetFaction(IMyFaction faction, bool create = true)
        {
            if (faction == null) return null;

            Faction fact;
            if (!Factions.TryGetValue(faction.FactionId, out fact))
            {
                fact = new Faction()
                {
                    FactionTag = faction.Tag,
                    Name = faction.Name,
                    Id = faction.FactionId
                };
                if(create) Factions.Add(faction.FactionId, fact);
            }
            return fact;
        }

        public static Faction GetFaction(string faction, bool create = true)
        {
            return GetFaction(Utilities.GetFactionByTag(faction), create);
        }

        public static void UpdateOnlineTime()
        {
            DateTime currentTimestamp = DateTime.Now;
            foreach (Hunter hunter in Players.Values)
            {
                // Delete unknown Identities
                IMyIdentity identity = Utilities.GetPlayerIdentity(hunter.Id);
                if (identity == null)
                {
                    Removes.Add(hunter.Id);
                    continue;
                }

                IMyPlayer player = Utilities.IdentityToPlayer(identity);
                if (player != null)
                {
                    hunter.Name = identity.DisplayName;
                    hunter.OnlineMinutes++;
                    hunter.LastSeen = currentTimestamp;
                }
            }

            foreach(long l in Removes) Players.Remove(l);
            Removes.Clear();
        }

        public static void RefreshHunterFactionData()
        {
            
            foreach(Faction fact in Factions.Values)
            {
                fact.Members.Clear();
                IMyFaction myFaction = MyAPIGateway.Session.Factions.TryGetFactionById(fact.Id);
                // Delete old Factions
                if (myFaction == null)
                {
                    Removes.Add(fact.Id);
                    continue;
                }
                fact.Name = myFaction.Name;
                fact.FactionTag = myFaction.Tag;
            }
            foreach (long l in Removes) Factions.Remove(l);
            Removes.Clear();

            foreach (Hunter hunter in Players.Values)
            {
                // Delete unknown Identities
                IMyIdentity identity = Utilities.GetPlayerIdentity(hunter.Id);
                if(identity == null)
                {
                    Removes.Add(hunter.Id);
                    continue;
                }

                string oldFaction = hunter.FactionTag;

                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(identity.IdentityId);
                if(faction == null)
                {
                    hunter.FactionTag = null;
                } else
                {
                    hunter.FactionTag = faction.Tag;
                    GetFaction(faction).Members.Add(hunter.Name);
                }

                if(oldFaction != hunter.FactionTag)
                {
                    hunter.GraceTime = DateTime.Now;
                    hunter.Graced = true;
                } else if (hunter.Graced && hunter.GraceTime != null && DateTime.Now >= hunter.GraceTime.AddMinutes(Config.Instance.FactionChangeGraceTimeMinutes))
                {
                    hunter.Graced = false;
                    IMyPlayer player = Utilities.GetPlayer(hunter.Name);
                    if (player != null)
                        Utilities.ShowChatMessage("Your grace period is over.", player.IdentityId);
                }

                if(hunter.Banned && hunter.BannedUntil != null && DateTime.Now >= hunter.BannedUntil) {
                    hunter.Banned = false;
                    IMyPlayer player = Utilities.GetPlayer(hunter.Name);
                    if (player != null)
                        Utilities.ShowChatMessage("Your suspension is over.", player.IdentityId);
                }
            }
            foreach (long l in Removes) Players.Remove(l);
            Removes.Clear();
        }
    }
}
