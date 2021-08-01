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

namespace Bountyhunter.Store
{
    public class Bounties
    {

        public static BountiesDefinition Instance;


        public static void Load()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage("Bounties.xml", typeof(BountiesDefinition)))
            {
                try
                {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("Bounties.xml", typeof(BountiesDefinition));
                    var xmlData = reader.ReadToEnd();
                    Instance = MyAPIGateway.Utilities.SerializeFromXML<BountiesDefinition>(xmlData);
                    reader.Dispose();
                    Logging.Instance.WriteLine("Bounties found and loaded");
                }
                catch (Exception e)
                {
                    Logging.Instance.WriteLine("Bounties loading failed");
                }
            }

            ValidateData();
        }

        private static void ValidateData()
        {
            if (Instance == null) Instance = new BountiesDefinition();
            Save();
        }

        public static void Save()
        {
            try
            {
                Logging.Instance.WriteLine("Serializing Bounties to XML... ");
                string xml = MyAPIGateway.Utilities.SerializeToXML(Instance);
                Logging.Instance.WriteLine("Writing Bounties to disk... ");
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage("Bounties.xml", typeof(BountiesDefinition));
                writer.Write(xml);
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                Logging.Instance.WriteLine("Error saving Bounties XML!" + e.StackTrace);
            }
        }
    }
}
