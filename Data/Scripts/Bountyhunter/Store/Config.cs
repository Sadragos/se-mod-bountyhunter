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

namespace Bountyhunter.Store
{
    public class Config
    {

        public static FileConfig Instance;


        public static void Load()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage("Config.xml", typeof(FileConfig)))
            {
                try
                {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("Config.xml", typeof(FileConfig));
                    var xmlData = reader.ReadToEnd();
                    Instance = MyAPIGateway.Utilities.SerializeFromXML<FileConfig>(xmlData);
                    reader.Dispose();
                    Logging.Instance.WriteLine("Config found and loaded");
                }
                catch (Exception e)
                {
                    Logging.Instance.WriteLine("Config loading failed");
                }
            }

            ValidateData();
            Save();
        }

        private static void ValidateData()
        {
            if (Instance == null) Instance = new FileConfig();
            if(Instance.DeathCauseReplacements == null || Instance.DeathCauseReplacements.Count == 0)
            {
                Instance.DeathCauseReplacements = new List<DeathCauseReplacement>();
                Instance.DeathCauseReplacements.Add(new DeathCauseReplacement()
                {
                    PlayerName = "*",
                    AsAttacker = false,
                    Replacers = new List<ReplaceEntry>()
                    {
                        new ReplaceEntry("Bullet", "Gunfire"),
                        new ReplaceEntry("Grid", "Collision"),
                        new ReplaceEntry("Grind", "Grinding"),
                        new ReplaceEntry("Weld", "Welding"),
                        new ReplaceEntry("Drill", "Drill"),
                        new ReplaceEntry("Suicide", "Suicide"),
                        new ReplaceEntry("Accident", "Accident"),
                        new ReplaceEntry("Temperature", "Temperature"),
                        new ReplaceEntry("Floating Object", "Floating Object"),
                        new ReplaceEntry("Thrust", "Thrust"),
                        new ReplaceEntry("Nuke", "Nuke"),
                        new ReplaceEntry("Rocket", "Rocket")
                    }
                });
                Instance.DeathCauseReplacements.Add(new DeathCauseReplacement()
                {
                    PlayerName = "Sadragos",
                    AsAttacker = true,
                    Replacers = new List<ReplaceEntry>()
                    {
                        new ReplaceEntry("Bullet", "Mighty BUllets"),
                        new ReplaceEntry("Grid", "Crushing")
                    }
                });
                Save();
            }
        }

        public static void Save()
        {
            try
            {
                Logging.Instance.WriteLine("Serializing Config to XML... ");
                string xml = MyAPIGateway.Utilities.SerializeToXML(Instance);
                Logging.Instance.WriteLine("Writing Config to disk... ");
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("Config.xml", typeof(FileConfig));
                writer.Write(xml);
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine("Error saving Config XML!" + e.StackTrace);
            }
        }
    }
}
