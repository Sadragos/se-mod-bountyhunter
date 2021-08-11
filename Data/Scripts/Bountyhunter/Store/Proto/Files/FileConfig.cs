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
        public bool TakeCreditFromBank = true;

        [ProtoMember(6)]
        public bool CreditsAsItem = false;

        [ProtoMember(7)]
        public bool SpawnRewardDropPods = false;


        [ProtoMember(8)]
        public string CurrencyName = " SC";

        [ProtoMember(9)]
        public bool PlaceBountiesOnSelf = true;

        [ProtoMember(10)]
        public bool PlaceBountiesOnAllies = true;

        [ProtoMember(11)]
        public bool ClaimBountiesFromAllies = false;

        [ProtoMember(12)]
        public int DeathListEntries = 10;

        [ProtoMember(13)]
        public bool KillFeed = true;

        [ProtoMember(14)]
        public bool IncludeBountiesInKillFeed = true;

        [ProtoMember(15)]
        public bool CountSuicides = false;

        [ProtoMember(16)]
        public float FloatAmountBuffer = 0.000001f;

        [ProtoMember(17)]
        public float MinPayout = 0.01f;

        [ProtoMember(18)]
        public bool AnnouncyBounties = true;

        [ProtoMember(19)]
        public bool AnnouncyBountiesDeatils = true;

        [ProtoMember(20)]
        public int DamageBlockMessageBatchingSeconds = 5;

        [ProtoMember(21)]
        public bool CountGrindingAsDestroy = true;
    }
}
