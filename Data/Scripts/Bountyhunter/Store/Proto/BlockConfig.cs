using System;
using ProtoBuf;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class BlockConfig
    {

        [ProtoMember(1)]
        [XmlAttribute]
        public string BlockId;

        [ProtoMember(2)]
        [XmlAttribute]
        public float Value;

        [ProtoMember(3)]
        public List<Item> Components;

        [ProtoMember(4)]
        public List<string> Alias;

        [ProtoMember(5)]
        [XmlAttribute]
        public string BountyId;

        public BlockConfig(string blockId)
        {
            BlockId = blockId;
            Value = 0;
            Components = new List<Item>();
            Alias = new List<string>();
        }

        public BlockConfig()
        {
        }

        public new string ToString()
        {
            if (Alias != null && Alias.Count > 0) return Alias[0];
            return BlockId.Replace("MyObjectBuilder_", "");
        }
    }
}