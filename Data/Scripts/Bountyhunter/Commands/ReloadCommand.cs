using Bountyhunter.Store;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI;

namespace Bountyhunter.Commands
{
    class ReloadCommand : AbstactCommandHandler
    {

        public ReloadCommand() : base("reload", MyPromoteLevel.Admin) {

        }

        public override string ArgumentDescription => "[c/v/p/k]\nThis wil reload relevant XML files from the Disk. " +
            "This is usefull if you hgave changed them with an external Editor. " +
            "Make sure to use this command quickly after making your changes, as the file is saved everytime your world is saved, which can result in the loss of your changes. " +
            "To reload only a specific File you can use an argument. c= Config, v = Values, p = Participants, k = Killmessages";

        public override void HandleCommand(IMyPlayer player, string[] arguments)
        {
            if (arguments != null && arguments.Length == 1)
            {
                switch(arguments[0])
                {
                    case "c":
                        Config.Load();
                        if (player != null) SendMessage(player, "Config loaded.");
                        break;
                    case "v":
                        Values.Load();
                        if (player != null) SendMessage(player, "Values loaded.");
                        break;
                    case "p":
                        Participants.Load();
                        if (player != null) SendMessage(player, "Participants loaded.");
                        break;
                    case "k":
                        Killmessages.Load();
                        if (player != null) SendMessage(player, "Killmessages loaded.");
                        break;
                    default:
                        WrongArguments(player);
                        break;
                }
            }
            else
            {
                Config.Load();
                if (player != null) SendMessage(player, "Config loaded.");
                Values.Load();
                if (player != null) SendMessage(player, "Values loaded.");
                Participants.Load();
                if (player != null) SendMessage(player, "Participants loaded.");
                Killmessages.Load();
                if (player != null) SendMessage(player, "Killmessages loaded.");
            }
        }
    }
}
