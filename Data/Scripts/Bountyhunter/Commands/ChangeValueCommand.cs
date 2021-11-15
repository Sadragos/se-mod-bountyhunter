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
using Bountyhunter.Utils;
using Bountyhunter.Store;
using static Bountyhunter.Store.Values;
using Bountyhunter.Store.Proto;

namespace Bountyhunter.Commands
{
    class ChangeValueCommand : AbstactCommandHandler
    {

        public ChangeValueCommand() : base("changevalue", MyPromoteLevel.Admin) {

        }

        public override string ArgumentDescription => "<item/block> <searchtext> <value> [enabledAsBounty]\n" +
            "Changes the value of an Item or a Block. You can use the same item/block search parameters as in the /bh value command. " +
            "enabledAsBounty can be true/false. If not given the value will not be changed.";

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if (arguments.Length >= 3 && arguments[0].Equals("item"))
            {
                List<ItemConfig> items = FindItemFuzzy(arguments[1]);

                StringBuilder builder = new StringBuilder();
                if (items.Count != 1)
                {
                    builder.Append("No Item with that name found.");
                    if (items.Count > 1)
                    {
                        builder.Append(" Did you mean...\n");
                        foreach (ItemConfig id in items)
                        {
                            builder.Append("- [")
                                .Append(id.BountyId)
                                .Append("] ")
                                .Append(id.ToString())
                                .Append("\n");
                        }
                    }
                    SendMessage(player, builder.ToString());
                    return;
                }

                ItemConfig item = items.FirstOrDefault();
                float oldValue = item.Value;
                bool oldAllowed = item.AllowedAsBounty;

                item.Value = Utilities.ParseNumber(arguments[2]);
                bool enabled;
                if(arguments.Length == 4 && bool.TryParse(arguments[3], out enabled))
                {
                    item.AllowedAsBounty = enabled;
                }
                Values.Save();

                builder.Append("The price of ")
                    .Append(item.ToString())
                    .Append(" ");
                if (oldValue != item.Value)
                    builder.Append("changed from ")
                        .Append(oldValue.ToString("0.0000"))
                        .Append(" to ")
                        .Append(item.Value.ToString("0.0000"));
                else
                    builder.Append("remained at ")
                        .Append(item.Value.ToString("0.0000"));
                builder.Append(". Its allowed State ");
                if (oldAllowed != item.AllowedAsBounty)
                    builder.Append("changed from ")
                        .Append(oldAllowed)
                        .Append(" to ")
                        .Append(item.AllowedAsBounty);
                else
                    builder.Append("stayed at ")
                        .Append(item.AllowedAsBounty);
                builder.Append(".");


                SendMessage(player, builder.ToString());

            }
            else if (arguments.Length == 3 && arguments[0].Equals("block"))
            {
                List<BlockConfig> items = FindBlockFuzzy(arguments[1]);

                StringBuilder builder = new StringBuilder();
                if (items.Count != 1)
                {
                    builder.Append("No Item with that name found.");
                    if (items.Count > 1)
                    {
                        builder.Append(" Did you mean...\n");
                        foreach (BlockConfig id in items)
                        {
                            builder.Append("- [")
                                .Append(id.BountyId)
                                .Append("] ")
                                .Append(id.ToString())
                                .Append("\n");
                        }
                    }
                    SendMessage(player, builder.ToString());
                    return;
                }

                BlockConfig blockConfig = items.FirstOrDefault();
                float oldValue = blockConfig.Value;
                blockConfig.Value = Utilities.ParseNumber(arguments[2]);
                Values.Save();

                builder.Append("The price of ")
                    .Append(blockConfig.ToString())
                    .Append(" ");
                if (oldValue != blockConfig.Value)
                    builder.Append("changed from ")
                        .Append(oldValue.ToString("0.0000"))
                        .Append(" to ")
                        .Append(blockConfig.Value.ToString("0.0000"));
                else
                    builder.Append("remained at ")
                        .Append(blockConfig.Value.ToString("0.0000"));
                builder.Append(".");


                SendMessage(player, builder.ToString());
            } else
            {
                WrongArguments(player);
            }
        }
    }
}
