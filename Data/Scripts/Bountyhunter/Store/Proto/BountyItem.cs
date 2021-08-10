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
        public float Claimed = 0;

        [ProtoMember(4)]
        [XmlAttribute]
        public float Worth = 0;

        public BountyItem(string itemId) : base(itemId)
        {
        }

        public BountyItem()
        {
        }

        public void RecalculateWorth()
        {
            Worth = Remaining * Values.ItemValue(ItemId);
        }

        [XmlIgnore]
        [ProtoIgnore]
        public float Remaining {  get { return Value - Claimed;  } }
    }
}
