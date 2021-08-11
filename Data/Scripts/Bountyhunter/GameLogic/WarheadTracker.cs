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
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Warhead), false)]
    class WarheadTracker : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase builder)
        {
            base.Init(builder);
        }

        public override void OnRemovedFromScene()
        {

            base.OnRemovedFromScene();
            if (Entity == null) return;
            if (!MyAPIGateway.Multiplayer.IsServer) return;
            IMyWarhead warhead = (IMyWarhead)Entity;
            if (warhead == null) return;
            //if (!(warhead.IsArmed || warhead.IsCountingDown || warhead.DetonationTime == 0)) return;

            ExplosionInfo data = new ExplosionInfo
            {
                LastPosition = Entity.GetPosition(),
                DateTime = DateTime.Now,
                Type = "Warhead",
                Radius = 32,
                Entity = Entity,
                OwnerId = warhead.OwnerId != 0 ? warhead.OwnerId : warhead.SlimBlock.BuiltBy
            };

            DeathHandler.Explosions.Add(data);
        }
    }
}
