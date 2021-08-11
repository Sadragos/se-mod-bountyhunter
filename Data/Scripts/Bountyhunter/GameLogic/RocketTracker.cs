using Bountyhunter.Utils;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace Bountyhunter.GameLogic
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Missile), false)]
    class RocketTracker : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase builder)
        {
            base.Init(builder);
        }

        public override void OnRemovedFromScene()
        {
            try
            {
                if (Entity == null) return;
                if (!MyAPIGateway.Multiplayer.IsServer) return;

                IMyEntity launcherEnt = null;
                MyObjectBuilder_Missile ob = (MyObjectBuilder_Missile)Entity.GetObjectBuilder();
                MyAPIGateway.Entities.TryGetEntityById(ob.LauncherId, out launcherEnt);
                ExplosionInfo data = new ExplosionInfo
                {
                    LastPosition = Entity.GetPosition(),
                    DateTime = DateTime.Now,
                    Type = "Rocket",
                    Radius = 15,
                    Entity = Entity,
                    OwnerId = ob != null ? ob.Owner : 0
                };
                if (launcherEnt != null && launcherEnt is IMyUserControllableGun)
                {
                    var launcher = launcherEnt as IMyUserControllableGun;
                    data.OwnerId = launcher.OwnerId;
                }
                DeathHandler.Explosions.Add(data);
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine("Exception " + ex);
            }
        }
    }
}
