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
using ProtoBuf;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using Bountyhunter.Store.Proto.Files;

namespace Bountyhunter.Store
{
    public class Killmessages
    {

        public static FileMessages Instance;


        public static void Load()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage("Killmessages.xml", typeof(FileMessages)))
            {
                try
                {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("Killmessages.xml", typeof(FileMessages));
                    var xmlData = reader.ReadToEnd();
                    Instance = MyAPIGateway.Utilities.SerializeFromXML<FileMessages>(xmlData);
                    reader.Dispose();
                    Logging.Instance.WriteLine("Killmessages found and loaded");
                }
                catch (Exception e)
                {
                    Logging.Instance.WriteLine("Killmessages loading failed");
                }
            }

            ValidateData();
            Save();
        }

        private static void ValidateData()
        {
            if (Instance == null) Instance = new FileMessages();
            if(Instance.Entries == null || Instance.Entries.Count == 0)
            {
                Instance.Entries = new List<CauseOfDeath>()
                {
                    new CauseOfDeath()
                    {
                        Reason = "Bullet",
                        ShortName = "Gunfire",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V got shot."),
                            new DeathMessage("*", "$V looket at the wrong side of a gun.")
                        },
                        AsAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V got shot by $A."),
                            new DeathMessage("*", "$A shot $V.")
                        },
                        AsVictim = new List<DeathMessage>()
                        {
                            new DeathMessage("Sadragos", "$V got killed by $A, finally!")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "Grind",
                        ShortName = "Grinder",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V ran into a grinder.")
                        },
                        AsAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V got ground down by $A.")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "Grid",
                        ShortName = "Collision",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V ran into a Grid too fast.")
                        },
                        AsAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V was crushed by $A's grid.")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "Weld",
                        ShortName = "Welder",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V burned himself to Death.")
                        },
                        AsAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$A burned $V.")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "Drill",
                        ShortName = "Drill",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V was drilled to Death.")
                        },
                        AsAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$A drilled $V.")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "Suicide",
                        ShortName = "Suicide",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V commited Suicide.")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "Accident",
                        ShortName = "Accident",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V had an accident.")
                        },
                        AsAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$A accidentally killed $V.")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "Temperature",
                        ShortName = "Temperature",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V froze to Death.")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "LowPressure",
                        ShortName = "Suffocation",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V suffocated.")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "Floating Object",
                        ShortName = "Floating Object",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V hit a Rock and died.")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "Thrust",
                        ShortName = "Thruster",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V looked into a Thruster.")
                        },
                        AsAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$A burned $V with a thruster.")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "Nuke",
                        ShortName = "Nuke",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V got nuked.")
                        },
                        AsAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$A nuked $V.")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "Rocket",
                        ShortName = "Rocket",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V was hit by a Rocket.")
                        },
                        AsAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$A shot $V with a Rocket.")
                        }
                    },
                    new CauseOfDeath()
                    {
                        Reason = "Explosion",
                        ShortName = "Explosion",
                        NoAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$V was blasted to pieces.")
                        },
                        AsAttacker = new List<DeathMessage>()
                        {
                            new DeathMessage("*", "$A exploded $V into pieces.")
                        }
                    }
                };
                Save();
            }
        }

        public static void Save()
        {
            try
            {
                Logging.Instance.WriteLine("Serializing Killmessages to XML... ");
                string xml = MyAPIGateway.Utilities.SerializeToXML(Instance);
                Logging.Instance.WriteLine("Writing Killmessages to disk... ");
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("Killmessages.xml", typeof(FileMessages));
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
