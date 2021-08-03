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
    }
}
