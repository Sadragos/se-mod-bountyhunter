using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class BountyItem : Item
    {

        [ProtoMember(3)]
        [XmlAttribute]
        public bool AllowedAsBounty;

        [ProtoMember(4)]
        public List<Item> Components;

        [ProtoMember(5)]
        public List<string> Alias;

        public BountyItem(string itemId)
        {
            ItemId = itemId;
            AllowedAsBounty = true;
            Value = 0;
            Components = new List<Item>();
            Alias = new List<string>();
        }

        public BountyItem()
        {
            Components = new List<Item>();
            Alias = new List<string>();
        }

        public new string ToString()
        {
            if (Alias != null && Alias.Count > 0) return Alias[0];
            return ItemId.Replace("MyObjectBuilder_", "");
        }
    }
}
