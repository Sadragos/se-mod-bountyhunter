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

                if (!int.TryParse(splits[0], out rgb[0]) || !int.TryParse(splits[1], out rgb[1]) || !int.TryParse(splits[2], out rgb[2]))
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
        public bool ClaimBountiesFromAlliesAndNeutrals = false;

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
        public bool AnnouncyBountiesWithName = true;

        [ProtoMember(21)]
        public bool CountGrindingAsDestroy = false;

        [ProtoMember(22)]
        public int RankingRefreshSeconds = 60;

        [ProtoMember(23)]
        public bool ShowLastLogin = true;

        [ProtoMember(24)]
        public bool ShowOnlineTime = true;

        [ProtoMember(25)]
        public int FactionChangeGraceTimeMinutes = 1440;

        [ProtoMember(26)]
        public int GridValueMinOwnershipPercent = 60;

        [ProtoMember(27)]
        public int MinItemAmount = 1;

        [ProtoMember(28)]
        public int MaxItemAmount = 10000000;

        [ProtoMember(29)]
        public int ReimburseBountiesAfterMinutes = 10080;

        [ProtoMember(30)]
        public int BountyDelaySeconds = 60;

        [ProtoMember(31)]
        public bool MonthlyReset = true;

        [ProtoMember(32)]
        public bool KeepMonthlyHistory = true;

        [ProtoMember(33)]
        public DateTime LastReset = DateTime.Now;
    }
}
