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
        // TODO 

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
            // TODO

            KillerInfo attacker = GetKiller(info);
            
        }

        private static void HandlePlayerDeath(IMyCharacter myCharacter, MyDamageInformation info)
        {
            Logging.Instance.WriteLine("Player died " + (myCharacter).ToString());
            IMyIdentity identity = Utilities.EntityToIdentity(myCharacter);
            if (identity == null) return;
            IdentityInfo victim = GetIdentity(identity);

            KillerInfo attacker = GetKiller(info);
            Logging.Instance.WriteLine("Git Killer");

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
            Logging.Instance.WriteLine("Reason " + reason);

            if (reason.Equals("Suicide") || (attacker.Info.Player != null && attacker.Info.Player.IdentityId.Equals(attacker.Info.Player.IdentityId)))
            {
                // Selbstverschuldeter Tod oder NPC
                attacker.IsAllied = true;
                attacker.IsSameFaction = true;
                attacker.Selfinflicted = true;
            }
            else if (attacker.Info.Faction != null && victim.Faction != null)
            {
                if (attacker.Info.Faction.FactionId.Equals(victim.Faction.FactionId))
                {
                    // Selbe oder verbündete Fraktion
                    attacker.IsAllied = true;
                    attacker.IsSameFaction = true;
                } else if(attacker.Info.Faction.IsFriendly(victim.Identity.IdentityId))
                {
                    attacker.IsAllied = true;
                }
            }

            float bounty = 0;
            if(victim.Hunter.Bounties.Count > 0)
            {
                // TODO Get bounties
            }

            
            if(!attacker.Selfinflicted && attacker.Info.Identity != null)
            {
                victim.Hunter.AddDeath(reason, attacker.Info.Identity.DisplayName);
                if(!attacker.Info.IsBot && attacker.Info.Hunter != null) attacker.Info.Hunter.AddKill(reason, victim.Hunter.Name);
            } else
            {
                victim.Hunter.AddDeath(reason, null);
            }

            if(Config.Instance.KillFeed)
            {
                // TODO Better messages
                string message = "";
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
    }

    public class IdentityInfo
    {
        public IMyIdentity Identity = null;
        public IMyFaction Faction = null;
        public IMyPlayer Player = null;
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
}
