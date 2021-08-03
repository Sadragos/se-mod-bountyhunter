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
        public bool AllowedAsBounty = true;

        [ProtoMember(4)]
        public List<Item> Components = new List<Item>();

        [ProtoMember(5)]
        public List<string> Alias = new List<string>();

        public BountyItem(string itemId)
        {
            ItemId = itemId;
        }

        public BountyItem() { }

        public new string ToString()
        {
            if (Alias != null && Alias.Count > 0) return Alias[0];
            return ItemId.Replace("MyObjectBuilder_", "");
        }
    }
}
