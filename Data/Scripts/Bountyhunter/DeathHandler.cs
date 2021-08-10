using Bountyhunter.Store;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Bountyhunter
{
    class DeathHandler
    {
        public static Dictionary<long, IdentityInfo> IdentityCache = new Dictionary<long, IdentityInfo>();

        // ETWAS IST GESTORBEN: TU WAS!

        public static void DestroyHandler(object target, MyDamageInformation info)
        {

            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            if (target == null)
                return;

            if (target is IMyCharacter)
            {
                HandlePlayerDeath(target as IMyCharacter, info);
            }
            else if(target is IMySlimBlock && (target as IMySlimBlock).FatBlock != null && (target as IMySlimBlock).FatBlock is IMyTerminalBlock)
            {
                HandleBlockDeath((target as IMySlimBlock).FatBlock as IMyTerminalBlock, info);
            }
        }

        private static void HandleBlockDeath(IMyTerminalBlock myTerminalBlock, MyDamageInformation info)
        {
            Logging.Instance.WriteLine("Block died " + myTerminalBlock.DisplayName);

            IMyIdentity identity = Utilities.EntityToIdentity(myTerminalBlock);
            if (identity == null) return;   // Unowned

            IdentityInfo victim = GetIdentity(identity);

            if (victim.Hunter == null) return;

            KillerInfo attacker = GetKiller(info);

            Logging.Instance.WriteLine("Victom " + victim.Hunter.ToString());
            Logging.Instance.WriteLine("Attacker " + attacker.Info.Hunter.ToString());

            // For blocks only count PVP Damage (because of repairs etc.)
            if (attacker.Info.Hunter.Id.Equals(victim.Hunter.Id) || (attacker.Info.Faction != null && attacker.Info.Faction.IsFriendly(victim.Player.IdentityId))) return;

            float bounty = 0;
            float value = Values.BlockValue(myTerminalBlock.BlockDefinition.TypeIdString);
            victim.Hunter.DamageReceived += value;
            if (!attacker.Selfinflicted && attacker.Info.Hunter != null && (!attacker.IsAllied || Config.Instance.ClaimBountiesFromAllies))
            {
                attacker.Info.Hunter.DamageDone += value;

                bounty += victim.Hunter.ClaimBounty(attacker.Info.Hunter, EBountyType.Damage, value);
                if (victim.Faction != null)
                {
                    Faction faction = Participants.GetFaction(victim.Faction);
                    faction.ClaimBounty(attacker.Info.Hunter, EBountyType.Damage, value);
                }

                if(bounty > 0)
                {
                    attacker.Info.PendingBountyAmount += bounty;
                    attacker.Info.PendingBountySeconds = Config.Instance.DamageBlockMessageBatchingSeconds;
                }
            }
            
        }

        private static void HandlePlayerDeath(IMyCharacter myCharacter, MyDamageInformation info)
        {
            IMyIdentity identity = Utilities.EntityToIdentity(myCharacter);
            if (identity == null) return;
            IdentityInfo victim = GetIdentity(identity);

            KillerInfo attacker = GetKiller(info);

            string reason = null;

            // Workaround for suicide
            if (info.Amount == 1000)
            {
                reason = "Suicide";
                if (!Config.Instance.CountSuicides) return;
            }

            // This will be an NPC - ignore it.
            if (reason == null && (info.Type.ToString() == "" || victim.Player.DisplayName == ""))
                return;

            reason = info.Type.String;
            if (reason.Equals("Environment"))
            {
                if (attacker.Entity == null) reason = "Fall";
                else if (attacker.Entity.ToString().ToLower().Contains("missile")) reason = "Rocket";
                else if (attacker.Entity is IMyFloatingObject) reason = "Floating";
                else if (attacker.Entity is IMyCubeGrid) reason = "Grid";
                else reason = "Fall";
            }

            if (reason.Equals("Suicide") || (attacker.Info.Player != null && attacker.Info.Player.IdentityId.Equals(identity.IdentityId)))
            {
                attacker.IsAllied = true;
                attacker.IsSameFaction = true;
                attacker.Selfinflicted = true;
            }
            else if (attacker.Info.Faction != null && victim.Faction != null)
            {
                if (attacker.Info.Faction.FactionId.Equals(victim.Faction.FactionId))
                {
                    attacker.IsAllied = true;
                    attacker.IsSameFaction = true;
                } else if(attacker.Info.Faction.IsFriendly(victim.Identity.IdentityId))
                {
                    attacker.IsAllied = true;
                }
            }

            float bounty = 0;
            if (!attacker.Info.IsBot && !attacker.Selfinflicted && attacker.Info.Hunter != null && (!attacker.IsAllied || Config.Instance.ClaimBountiesFromAllies))
            {

                bounty += victim.Hunter.ClaimBounty(attacker.Info.Hunter, EBountyType.Kill);
                if (victim.Faction != null) {
                    Faction faction = Participants.GetFaction(victim.Faction);
                    faction.ClaimBounty(attacker.Info.Hunter, EBountyType.Kill);
                }
            }

            if ((!Config.Instance.KillFeed || !Config.Instance.IncludeBountiesInKillFeed) && attacker.Info.Player != null && !attacker.Info.IsBot)
            {
                Utilities.ShowChatMessage("You've earned a bounty worth " + Formater.FormatCurrency(bounty) + ".", attacker.Info.Player.IdentityId);
            }

            
            if(!attacker.Selfinflicted && attacker.Info.Identity != null)
            {
                victim.Hunter.AddDeath(reason, attacker.Info.Identity.DisplayName, bounty);
 

                if(!attacker.Info.IsBot && attacker.Info.Hunter != null) attacker.Info.Hunter.AddKill(reason, victim.Hunter.Name, bounty);
                if (!attacker.Info.IsBot && !string.IsNullOrEmpty(attacker.Info.Hunter.FactionTag))
                {
                    Participants.GetFaction(attacker.Info.Faction).BountyClaimed += bounty;
                }
            } else
            {
                victim.Hunter.AddDeath(reason, null);
            }

            if(Config.Instance.KillFeed)
            {
                // TODO Better messages
                string message;
                if (attacker.Info.Identity != null && !attacker.Selfinflicted)
                {
                    message = attacker.Info.Identity.DisplayName + " killed " + victim.Hunter.Name + " by " + reason;
                }
                else
                {
                    message = victim.Hunter.Name + " died by " + reason + ".";
                }

                if(Config.Instance.IncludeBountiesInKillFeed && bounty > 0 && attacker.Info.Hunter != null)
                {
                    message += " " + attacker.Info.Hunter.Name + " earned a Bounty of " + Formater.FormatCurrency(bounty) + ".";
                }
                Utilities.ShowChatMessage(message);
            }
        }

        private static KillerInfo GetKiller(MyDamageInformation info)
        {
            IMyEntity entity;
            if (!MyAPIGateway.Entities.TryGetEntityById(info.AttackerId, out entity))
            {
                Logging.Instance.WriteLine("No attacking Entity found");
                return new KillerInfo()
                {
                    Info = GetIdentity(null)
                };
            } else
            {
                IMyIdentity identity = Utilities.EntityToIdentity(entity);
                return new KillerInfo()
                {
                    Info = GetIdentity(identity),
                    Entity = entity
                };
            }
        }

        public static IdentityInfo GetIdentity(IMyIdentity identity)
        {
            if(identity == null) return new IdentityInfo();
            if(!IdentityCache.ContainsKey(identity.IdentityId))
            {
                IdentityInfo info = new IdentityInfo();
                info.Identity = identity;
                info.Populate();
                IdentityCache.Add(identity.IdentityId, info);
            }
            return IdentityCache[identity.IdentityId];
        }

        public static void UpdateIdentityCache()
        {
            foreach(IdentityInfo info in IdentityCache.Values)
            {
                info.Populate();
            }
        }

        public static void CheckPendingMessages()
        {
            foreach(IdentityInfo info in IdentityCache.Values)
            {
                if(info.PendingBountyAmount > 0)
                {
                    info.PendingBountySeconds--;
                    if(info.PendingBountySeconds <= 0)
                    {
                        Utilities.ShowChatMessage("You've earned bounty worth " + Formater.FormatCurrency(info.PendingBountyAmount) + ".");
                        info.PendingBountyAmount = 0;
                    }
                }
            }
        }
    }

    public class IdentityInfo
    {
        public IMyIdentity Identity = null;
        public IMyFaction Faction = null;
        public IMyPlayer Player = null;
        public float PendingBountyAmount = 0;
        public int PendingBountySeconds = 0;
        public bool IsBot = false;
        public Hunter Hunter = null;

        public void Populate()
        {
            if (Identity != null)
            {
                Faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(Identity.IdentityId);
                if (Faction != null && Faction.IsEveryoneNpc())
                {
                    Faction = null;
                    IsBot = true;
                }
                if (!IsBot)
                {
                    Player = Utilities.IdentityToPlayer(Identity);
                }
                if (Player != null)
                {
                    Hunter = Participants.GetPlayer(Player);
                }
                Logging.Instance.WriteLine("populated " + Identity.DisplayName);
            }
        }
    }

    public class KillerInfo
    {
        public IMyEntity Entity;
        public IdentityInfo Info;
        public bool IsAllied = false;
        public bool IsSameFaction = false;
        public bool Selfinflicted = false;
    }

    public class BountyInfo
    {
        public IMyPlayer Reciever;
        public int BatchSeconds;
        public float Amount;
    }
}
