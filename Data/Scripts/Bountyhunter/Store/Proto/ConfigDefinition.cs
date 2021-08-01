using System;
using System.Collections.Generic;
using ProtoBuf;
using System.Xml.Serialization;
using VRageMath;

namespace Bountyhunter.Store.Proto
{
    [ProtoContract]
    [Serializable]
    public class ConfigDefinition
    {
        [ProtoMember(1)]
        public string BroadcastName;
        [ProtoMember(2)]
        public string BroadcastNameColor;

        [ProtoIgnore]
        [XmlIgnore]
        public Color BroadcastNameRealColor
        {
            get
            {
                if (BroadcastNameColor == null || !BroadcastNameColor.Contains(",")) return VRageMath.Color.OrangeRed;
                string[] splits = BroadcastNameColor.Split(',');
                int[] rgb = new int[3];
                if (splits.Length != 3) return VRageMath.Color.OrangeRed;
                
                if(!int.TryParse(splits[0], out rgb[0]) || !int.TryParse(splits[1], out rgb[1]) || !int.TryParse(splits[2], out rgb[2]))
                    return VRageMath.Color.OrangeRed;

                return new Color(rgb[0], rgb[1], rgb[2]);
            }
        }

        [ProtoMember(2)]
        public bool EnableFactionBounties;
        [ProtoMember(3)]
        public bool EnablePlayerBounties;

        [ProtoMember(4)]
        public bool EnableItemBounties;
        [ProtoMember(5)]
        public bool EnableCreditBounties;
        [ProtoMember(6)]
        public bool SpawnRewardDropPods;


        [ProtoMember(7)]
        public string CurrencyName;
    }
}