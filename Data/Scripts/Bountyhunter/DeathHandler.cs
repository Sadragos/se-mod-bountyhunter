using Bountyhunter.GameLogic;
using Bountyhunter.Store;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Bountyhunter
{
    class DeathHandler
    {
        public static Dictionary<long, IdentityInfo> IdentityCache = new Dictionary<long, IdentityInfo>();
        public static List<ExplosionInfo> Explosions = new List<ExplosionInfo>();
        public static List<BountyInfo> PendingBountyInfos = new List<BountyInfo>();

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
            } else
            {
                //BeforeDamage(target, ref info);
            }
        }

        private static void HandleBlockDeath(IMySlimBlock mySlimBlock, MyDamageInformation info)
        {
            if(mySlimBlock.FatBlock == null)
            {
                //Logging.Instance.WriteLine("  ABORT: No Fat Block for " + mySlimBlock.BlockDefinition.ToString());
                return;
            }
            //Logging.Instance.WriteLine("Block "+mySlimBlock.BlockDefinition.ToString()+"  Destroyed by " + info.AttackerId + " at " + mySlimBlock.FatBlock.GetPosition());

            IMyIdentity identity = Utilities.SlimToIdentity(mySlimBlock);
            if (identity == null) 
            {
                //Logging.Instance.WriteLine("  ABORT: Unknown Blockowner");
                return;
            }
            //Logging.Instance.WriteLine("  BO " + identity.DisplayName);

            IdentityInfo victim = GetIdentity(identity);
            if (victim.Hunter == null)
            {
                //Logging.Instance.WriteLine("  ABORT: Unknown Victim");
                return;
            }
            //Logging.Instance.WriteLine("  Victim Hunter found");

            KillerInfo attacker = GetKiller(info, mySlimBlock.FatBlock.GetPosition());
            if (attacker.Info.Hunter == null)
            {
                //Logging.Instance.WriteLine("  ABORT: Unknown Attacker");
                return;
            }
            //Logging.Instance.WriteLine("  Attacker " + attacker.Info.Hunter.ToString());

            // For blocks only count PVP Damage (because of repairs etc.)
            if (attacker.Info.Identity.IdentityId.Equals(victim.Identity.IdentityId) || (attacker.Info.Faction != null && attacker.Info.Faction.IsFriendly(victim.Identity.IdentityId)))
            {
                //Logging.Instance.WriteLine("  ABORT: Selfharm or Allied");
                return;
            }
            //Logging.Instance.WriteLine("  No Self or Allied harm.");

            if (!Config.Instance.CountGrindingAsDestroy && info.Type.String.Equals("Grind"))
            {
                //Logging.Instance.WriteLine("  ABORT: Grind check failed");
                return;
            }
            //Logging.Instance.WriteLine("  Grind check passed.");

            float bounty = 0;
            float value = Values.BlockValue(mySlimBlock.BlockDefinition.ToString());
            //Logging.Instance.WriteLine("  BlockValue " + value);

            // TODO Value of Cargo

            victim.Hunter.DamageReceived += value;
            attacker.Info.Hunter.DamageDone += value;
            //Logging.Instance.WriteLine("  Attributed Damage");

            bounty += victim.Hunter.ClaimBounty(attacker.Info.Hunter, EBountyType.Damage, value);
            //Logging.Instance.WriteLine("  Claimed Personal Bounty");
            if (victim.Faction != null)
            {
                Faction faction = Participants.GetFaction(victim.Faction);
                bounty += faction.ClaimBounty(attacker.Info.Hunter, EBountyType.Damage, value);
                //Logging.Instance.WriteLine("  Claimed Faction Bounty");
            } else
            {
                //Logging.Instance.WriteLine("  No Faction for Factionbounties");
            }
            

            NotifyBounty(bounty, attacker);
            //Logging.Instance.WriteLine("  Notification");
        }

        internal static void AfterDamage(object target, MyDamageInformation info)
        {
            if (target is IMySlimBlock && (target as IMySlimBlock).IsDestroyed)
            {
                HandleBlockDeath(target as IMySlimBlock, info);
            }
        }

        private static void HandlePlayerDeath(IMyCharacter myCharacter, MyDamageInformation info)
        {
            IMyIdentity identity = Utilities.EntityToIdentity(myCharacter);
            if (identity == null) return;
            IdentityInfo victim = GetIdentity(identity);

            KillerInfo attacker = GetKiller(info, myCharacter.GetPosition());

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
                if (attacker.Entity == null) reason = "Accident";
                else if (attacker.Entity is IMyFloatingObject) reason = "Floating Object";
                else if (attacker.Entity is IMyCubeGrid) reason = "Grid";
                else if (attacker.Entity is IMyThrust) reason = "Thrust";
                else if (attacker.Entity is IMyWarhead) reason = "Nuke";
                else if (attacker.Entity.ToString().ToLower().Contains("missile")) reason = "Rocket";
                else reason = "Accident";
            }

            if (reason.Equals("Suicide") || (attacker.Info.Identity != null && attacker.Info.Identity.IdentityId.Equals(identity.IdentityId)))
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
                    bounty += faction.ClaimBounty(attacker.Info.Hunter, EBountyType.Kill);
                }
            }

            NotifyBounty(bounty, attacker);

            
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

        private static void NotifyBounty(float bounty, KillerInfo attacker)
        {
            if (bounty > 0 && attacker.Info.Player != null && !attacker.Info.IsBot)
            {
                BountyInfo bountyInfo = PendingBountyInfos.Find(pb => pb.Reciever.IdentityId.Equals(attacker.Info.Player.IdentityId));
                if(bountyInfo == null)
                {
                    bountyInfo = new BountyInfo()
                    {
                        Reciever = attacker.Info.Player,
                        Amount = bounty,
                        Ticks = 5
                    };
                    PendingBountyInfos.Add(bountyInfo);
                } else
                {
                    bountyInfo.Amount += bounty;
                    bountyInfo.Ticks = 5;
                }
            }
        }

        private static KillerInfo GetKiller(MyDamageInformation info, Vector3D position)
        {
            IMyEntity entity;
            if(info.AttackerId == 0 && position != Vector3D.Zero)
            {
                Logging.Instance.WriteLine("  No Attacker ID - find explosions");
                foreach (ExplosionInfo explosion in Explosions)
                {
                    if((explosion.LastPosition - position).Length() <= explosion.Radius)
                    {

                        IMyIdentity myIdentity = Utilities.CubeBlockBuiltByToIdentity(explosion.OwnerId);
                        if(myIdentity != null)
                        {
                            Logging.Instance.WriteLine("  Found Explosion nearby!");
                            return new KillerInfo()
                            {
                                Info = GetIdentity(myIdentity),
                                Entity = explosion.Entity
                            };
                        }
                    }
                }
                Logging.Instance.WriteLine("  No Explosion nearby...");
            }
            else if (MyAPIGateway.Entities.TryGetEntityById(info.AttackerId, out entity))
            {
                IMyIdentity identity = Utilities.EntityToIdentity(entity);
                return new KillerInfo()
                {
                    Info = GetIdentity(identity),
                    Entity = entity
                };
            } 

            Logging.Instance.WriteLine("  No attacking Entity found");
            return new KillerInfo()
            {
                Info = GetIdentity(null)
            };
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

        public static void CleanupExplosions()
        {
            if (Explosions.Count <= 0) return;

            DateTime delete = DateTime.Now;
            delete.AddSeconds(3);
            for (int i = Explosions.Count - 1; i >= 0; i--)
            {
                if (Explosions[i].DateTime <= delete) Explosions.RemoveAt(i);
            }
        }

        public static void CleanupBounties()
        {
            if (PendingBountyInfos.Count <= 0) return;

            for (int i = PendingBountyInfos.Count - 1; i >= 0; i--)
            {
                BountyInfo bi = PendingBountyInfos[i];
                MyVisualScriptLogicProvider.ClearNotifications(bi.Reciever.IdentityId);
                Utilities.ShowNotification("You've earned bounty worth [" + Formater.FormatCurrency(bi.Amount) + "].", bi.Reciever.IdentityId, 2000);
                if (bi.Ticks-- <= 0) PendingBountyInfos.RemoveAt(i);
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
                if (!IsBot)
                {
                    Hunter = Participants.GetPlayer(Identity);
                }
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
        public float Amount;
        public int Ticks;
    }
}
