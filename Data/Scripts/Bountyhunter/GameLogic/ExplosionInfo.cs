using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Bountyhunter.GameLogic
{
    public struct ExplosionInfo
    {
        public IMyEntity Entity;
        public long OwnerId;
        public int Radius;
        public string Type;
        public DateTime DateTime;
        public Vector3D LastPosition;

    }
}
