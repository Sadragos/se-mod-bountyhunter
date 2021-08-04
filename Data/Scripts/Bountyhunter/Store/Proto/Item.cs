using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class Item
    {
        [ProtoMember(1)]
        [XmlAttribute]
        public string ItemId;

        [ProtoMember(2)]
        [XmlAttribute]
        public float Value = 0;


        public Item(string itemId)
        {
            ItemId = itemId;
        }

        public Item(string itemId, float value) : this(itemId)
        {
            Value = value;
        }

        public Item()
        {
        }

        public bool IsOre { get { return ItemId.StartsWith("MyObjectBuilder_Ore"); } }
        public bool IsIngot { get { return ItemId.StartsWith("MyObjectBuilder_Ingot"); } }

        public bool HasFractions { get { return IsOre || IsIngot; } }
    }
}
