using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using System.Xml.Serialization;
using static Sandbox.Definitions.MyCubeBlockDefinition;
using ProtoBuf;

namespace Bountyhunter.Store.Proto.Files
{
    [ProtoContract]
    [Serializable]
    public class FileMessages
    {
        [ProtoMember(1)]
        public List<CauseOfDeath> Entries = new List<CauseOfDeath>();


        public string GetName(string cause)
        {
            CauseOfDeath causeOfDeath = Entries.Find(e => e.Reason.Equals(cause));
            if (causeOfDeath == null) return cause;
            return causeOfDeath.ShortName;
        }

        public string GetMessage(string cause, string victim, string attacker = null)
        {
            if (cause == null || victim == null) return null;

            CauseOfDeath causeOfDeath = Entries.Find(e => e.Reason.Equals(cause));
            if (causeOfDeath == null) return null;

            if (attacker == null)
            {
                List<DeathMessage> messages = causeOfDeath.NoAttacker.FindAll(c => c.Enabled && c.Player.Equals(victim));
                if (messages == null || messages.Count == 0)
                    messages = causeOfDeath.NoAttacker.FindAll(c => c.Enabled && c.Player.Equals("*"));
                if (messages == null || messages.Count == 0)
                    return null;

                DeathMessage message = messages[VRage.Utils.MyUtils.GetRandomInt(0, messages.Count)];
                return message.Message.Replace("$V", victim);
            }
            else
            {
                List<DeathMessage> messages = causeOfDeath.AsAttacker.FindAll(c => c.Enabled && c.Player.Equals(attacker));
                if (messages == null || messages.Count == 0)
                    messages = causeOfDeath.AsVictim.FindAll(c => c.Enabled && c.Player.Equals(victim));
                if (messages == null || messages.Count == 0)
                    messages = causeOfDeath.AsAttacker.FindAll(c => c.Enabled && c.Player.Equals("*"));
                if (messages == null || messages.Count == 0)
                    return null;

                DeathMessage message = messages[VRage.Utils.MyUtils.GetRandomInt(0, messages.Count)];
                return message.Message.Replace("$V", victim).Replace("$A", attacker);
            }
        }
    }
}
