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

namespace Bountyhunter.Store.Proto.Files
{
    [ProtoContract]
    [Serializable]
    public class FileValues
    {
        [ProtoMember(1)]
        public List<ItemConfig> ItemValues = new List<ItemConfig>();

        [ProtoMember(2)]
        public List<BlockConfig> BlockValues = new List<BlockConfig>();

    }
}
