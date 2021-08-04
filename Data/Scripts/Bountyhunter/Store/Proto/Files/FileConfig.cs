using System;
using System.Collections.Generic;
using ProtoBuf;
using System.Xml.Serialization;
using VRageMath;

namespace Bountyhunter.Store.Proto.Files
{
    [ProtoContract]
    [Serializable]
    public class FileConfig
    {
        [ProtoMember(1)]
        public string BroadcastName = "Bountyhunt";

        [ProtoMember(2)]
        public string BroadcastNameColor = "255,180,20";

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
        public bool EnableFactionBounties = true;

        [ProtoMember(3)]
        public bool EnablePlayerBounties = true;

        [ProtoMember(4)]
        public bool EnableItemBounties = true;

        [ProtoMember(5)]
        public bool EnableCreditBounties = true;

        [ProtoMember(6)]
        public bool CreditsAsItem = false;

        [ProtoMember(7)]
        public bool SpawnRewardDropPods = false;


        [ProtoMember(8)]
        public string CurrencyName = "sc";

        [ProtoMember(9)]
        public bool PlaceBountiesOnSelf = false;
        [ProtoMember(10)]
        public bool PlaceBountiesOnAllies = false;
        [ProtoMember(11)]
        public bool ClaimBountiesFromAllies = false;

    }
}
