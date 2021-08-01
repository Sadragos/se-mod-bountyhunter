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
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;

namespace Bountyhunter.Commands
{
    public abstract class AbstactCommandHandler
    {

        public MyPromoteLevel RequiredRank = MyPromoteLevel.None;

        public string CommandPrefix;

        public abstract void HandleCommand(IMyPlayer player, string[] arguments);

        public bool HasRank(IMyPlayer player)
        {
            return HasRank(player, RequiredRank);
        }

        public bool HasRank(IMyPlayer player, MyPromoteLevel rank)
        {
            return player.PromoteLevel.CompareTo(rank) >= 0;
        }

        protected AbstactCommandHandler(string commandPrefix)
        {
            CommandPrefix = commandPrefix;
        }

        protected AbstactCommandHandler(string commandPrefix, MyPromoteLevel requiredRank)
        {
            RequiredRank = requiredRank;
            CommandPrefix = commandPrefix;
        }

        protected void SendMessage(IMyPlayer player, string errorMessage)
        {
            Utilities.ShowChatMessage(errorMessage, player.IdentityId);
        }

        public string[] GetArguments(string arguments)
        {
            if (arguments == null || arguments.Length == 0) return new string[] { };
            if (arguments.Contains(" "))
            {
                string[] temp = arguments.Split(' ');
                if (!arguments.Contains("\"")) return temp;

                List<string> resultList = new List<string>();
                string escapedWord = null;
                foreach(string s in temp)
                {
                    if (escapedWord == null && s.StartsWith("\""))
                    {
                        escapedWord = s.Substring(1);
                    } else if ( escapedWord != null && s.EndsWith("\""))
                    {
                        escapedWord += " " + s.Substring(0, s.Length - 1);
                        resultList.Add(escapedWord);
                        escapedWord = null;
                    } else if (escapedWord != null)
                    {
                        escapedWord += " " + s;
                    } else
                    {
                        resultList.Add(s);
                    }
                }
                return resultList.ToArray();

            }
            return new string[] { arguments };
        }
    }
}
