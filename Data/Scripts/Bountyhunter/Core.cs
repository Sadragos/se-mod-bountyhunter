using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using Bountyhunter.Commands;
using Bountyhunter.Data.Proto;
using Bountyhunter.Store;
using Bountyhunter.Store.Proto;
using Bountyhunter.Utils;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using IMyCargoContainer = Sandbox.ModAPI.IMyCargoContainer;

//using Sandbox.ModAPI.Ingame;

namespace Bountyhunter
{
    [MySessionComponentDescriptor(
        MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation | MyUpdateOrder.Simulation )]
    public class Core : MySessionComponentBase
    {
        // Declarations
        private static readonly string version = "0.0.1";

        private static bool _initialized;

        public const ushort CLIENT_ID = 1699;
        public const ushort SERVER_ID = 1700;

        private static int interval = 0;

        public static List<AbstactCommandHandler> CommandHandlers = new List<AbstactCommandHandler>();

        override public void SaveData()
        {
            AbstactCommandHandler saveHandler = CommandHandlers.Find(ch => ch is SaveCommand);
            if(saveHandler != null)
            {
                saveHandler.HandleCommand(null, null);
            }
        }

        // Initializers
        private void Initialize( )
        {
            Logging.Instance.WriteLine(string.Format("Starting initialization Bountyhunter {0}", version));
            // Chat Line Event
            AddMessageHandler( );
            if (MyAPIGateway.Multiplayer.IsServer ) 
            {

                MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, DeathHandler.DestroyHandler);
                MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(0, DeathHandler.AfterDamage);
                MyVisualScriptLogicProvider.PrefabSpawnedDetailed += LootboxSpawner.NewSpawn;

                // CommandHandlers
                CommandHandlers.Add(new RecalculateCommand());
                CommandHandlers.Add(new SaveCommand());
                CommandHandlers.Add(new ReloadCommand());
                CommandHandlers.Add(new ValueCommand());
                CommandHandlers.Add(new NewBountyCommand());
                CommandHandlers.Add(new ClaimCommand());
                CommandHandlers.Add(new RankingCommand());
                CommandHandlers.Add(new ShowCommand());
                CommandHandlers.Add(new MeCommand());
                CommandHandlers.Add(new HelpCommand());

                AbstactCommandHandler reloadHandler = CommandHandlers.Find(ch => ch is ReloadCommand);
                if (reloadHandler != null)
                {
                    reloadHandler.HandleCommand(null, null);
                }
            } 

            Logging.Instance.WriteLine(string.Format("Script Initialized"));
        }
        
        // CLIENT hat eine Chatnachricht eingegeben. Prüfe, ob es ein Befehl ist
        public void HandleMessageEntered( string messageText, ref bool sendToOthers )
        {
            byte[] data = null;

            if (messageText.StartsWith("/bountyhunt") || messageText.StartsWith("/bh"))
            {
                Logging.Instance.WriteLine("Recieved Command " + messageText);

                data = Utilities.MessageToBytes(new ClientServerMessage()
                {
                    SteamId = MyAPIGateway.Session.Player.SteamUserId,
                    Message = messageText.Replace("/bountyhunt", "").Replace("/bh", "").Trim()
                });
                sendToOthers = false;
            }


            if (data != null)
            {
                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                {
                    MyAPIGateway.Multiplayer.SendMessageToServer(SERVER_ID, data);
                });
            }

        }

        // CLIENT hat Daten vom Server Erhalten. Entweder Chatnachricht oder Dialog
        public void HandleServerData( byte[ ] data )
        {
            Logging.Instance.WriteLine( string.Format( "Received Server Data: {0} bytes", data.Length ) );
            if ( MyAPIGateway.Multiplayer.IsServer && MyAPIGateway.Session.LocalHumanPlayer == null )
                return;

            ClientServerMessage item = Utilities.BytesToMessage(data);

            if (item == null)
                return;

            if ( item.Type.Equals("chat") )
            {
                MyAPIGateway.Utilities.ShowMessage(item.Sender, item.Message);
            } else if(item.Type.Equals("dialog"))
            {
                Dialog(item.Message, item.DialogTitle);
            }
        }

        // SERVER hat Daten vom Spieler erhalten - Vermutlich ein Befehl
        public void HandlePlayerData(byte[] data)
        {
           if ( !MyAPIGateway.Multiplayer.IsServer )
                return;

            Logging.Instance.WriteLine(string.Format("Received Player Data: {0} bytes", data.Length));
            ClientServerMessage request = Utilities.BytesToMessage(data);
            if (request == null)
                return;

            IMyPlayer player = Utilities.GetPlayer(request.SteamId);
            if (player == null)
                return;

            foreach (AbstactCommandHandler handler in CommandHandlers)
            {
                if(request.Message.StartsWith(handler.CommandPrefix))
                {
                    if (!handler.HasRank(player))
                    {
                        Utilities.ShowChatMessage("You don't have the permission to use this command.", player.IdentityId);
                        Logging.Instance.WriteLine(player.DisplayName + " tried using " + handler.CommandPrefix + " - missing permission");
                        return;
                    }
                    string arguments = request.Message.Substring(handler.CommandPrefix.Length).Trim();
                    Logging.Instance.WriteLine(player.DisplayName + " uses " + handler.CommandPrefix + "(" + arguments + ")");
                    handler.HandleCommand(player, handler.GetArguments(arguments));

                    return;
                }
            }
        }

        // Zeigt dem Spieler eine Dialog an
        public static void Dialog( string message, string title = null )
        {
            if (title == null) title = "Bountyhunt";
            MyAPIGateway.Utilities.ShowMissionScreen(title, "", "", message);
        }

        public void AddMessageHandler( )
        {
            //register all our events and stuff
            MyAPIGateway.Utilities.MessageEntered += HandleMessageEntered;
            MyAPIGateway.Multiplayer.RegisterMessageHandler( CLIENT_ID, HandleServerData );
            MyAPIGateway.Multiplayer.RegisterMessageHandler( SERVER_ID, HandlePlayerData );
        }

        public void RemoveMessageHandler( )
        {
            //unregister them when the game is closed
            MyAPIGateway.Utilities.MessageEntered -= HandleMessageEntered;
            MyAPIGateway.Multiplayer.UnregisterMessageHandler( CLIENT_ID, HandleServerData );
            MyAPIGateway.Multiplayer.UnregisterMessageHandler( SERVER_ID, HandlePlayerData );
        }

        public void UpdateBeforeEverySecond()
        {
            DeathHandler.CleanupExplosions();
            DeathHandler.CleanupBounties();
        }

        public void UpdateBeforeEveryMinute()
        {
            DeathHandler.UpdateIdentityCache();
            Participants.RefreshAllFactions();
        }

        // Overrides
        public override void UpdateBeforeSimulation( )
        {
            try
            {
                if ( MyAPIGateway.Session == null )
                    return;

                // Run the init
                if ( !_initialized )
                {
                    _initialized = true;
                    Initialize( );
                } else if(interval % 60 == 0)
                {
                    UpdateBeforeEverySecond();
                    if (interval % 3600 == 0)
                    {
                        UpdateBeforeEveryMinute();
                        interval = 0;
                    }
                }
                interval++;

            }
            catch ( Exception ex )
            {
                Logging.Instance.WriteLine( string.Format( "UpdateBeforeSimulation(): {0}", ex ) );
            }
        }

        

        public override void UpdateAfterSimulation( )
        {
        }

        protected override void UnloadData( )
        {
            try
            {
                RemoveMessageHandler( );

                if ( Logging.Instance != null )
                    Logging.Instance.Close( );
            }
            catch
            {
            }

            base.UnloadData( );
        }
    }
}