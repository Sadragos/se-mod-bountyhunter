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

        public static void RefreshAllFactions()
        {
            foreach(Faction fact in Factions.Values)
            {
                fact.Members.Clear();
                IMyFaction myFaction = MyAPIGateway.Session.Factions.TryGetFactionById(fact.Id);
                if (myFaction == null)
                {
                    Factions.Remove(fact.Id);
                    continue;
                }
                fact.Name = myFaction.Name;
                fact.FactionTag = myFaction.Tag;
            }

            foreach(Hunter hunter in Players.Values)
            {
                IMyIdentity player = Utilities.GetPlayerIdentity(hunter.Id);
                if(player == null)
                {
                    Players.Remove(hunter.Id);
                    continue;
                }
                hunter.Name = player.DisplayName;
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
                if(faction == null)
                {
                    hunter.FactionTag = null;
                } else
                {
                    hunter.FactionTag = faction.Tag;
                    GetFaction(faction).Members.Add(hunter.Name);
                }
            }
        }
    }
}
