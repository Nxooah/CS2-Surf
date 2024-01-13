using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities;
using CSurf.Database;
using CSurf.Helper;
using CSurf.Models;
using CSurf.Modules;
using CSurf.Modules.MapModule;
using Microsoft.Extensions.DependencyInjection;
using static CounterStrikeSharp.API.Core.Listeners;

namespace CSurf
{
    public class CSurfPlugin : BasePlugin
    {
        public override string ModuleName => "Classic Surf Plugin";

        public override string ModuleVersion => "0.0.1";

        private static CSurfPlugin _instance;
        public static CSurfPlugin Instance => _instance;

        private ServiceProvider _serviceProvider;
        private CSurfDatabaseService _databaseService;

        public override void Load(bool hotReload)
        {
            Server.ExecuteCommand("game_mode 0; game_type 3;");
            Server.ExecuteCommand("host_workshop_map 3073875025");
            Server.ExecuteCommand("sv_cheats 1;bot_kick;mp_ignore_round_win_conditions 1;sv_maxvelocity 7200;sv_accelerate 10;sv_airaccelerate 1000;sv_staminajumpcost 0;sv_staminalandcost 0;sv_staminamax 0;sv_autobunnyhopping 1;sv_enablebunnyhopping 1;sv_falldamage_scale 0;mp_warmup_end;mp_roundtime 45;mp_freezetime 0;mp_restartgame 1;mat_fullbright 0");
            Server.ExecuteCommand("mp_respawn_on_death_t 1; mp_respawn_on_death_ct 1; mp_autoteambalance 0; mp_limitteams 0; mp_solid_teammates 0; mp_humanteam CT; ");

            Server.ExecuteCommand("mp_ct_default_melee \"\"");
            Server.ExecuteCommand("mp_t_default_melee \"\"");

            Console.WriteLine("Classic Surf Plugin will be loaded...");
            if (_instance == null)
            {
                _instance = this;
            }

            _databaseService = new CSurfDatabaseService("mongodb://f1fty:4rM5Dp5K1nCQQ72Du2NM51qk8Nv@45.131.66.137:27017/", "csurf-eu-01");

            AddCommand("fifty", "sikerem amkk", (player, info) =>
            {
                if (player == null) return;
                player.PrintToConsole("X: " + player.PlayerPawn.Value.AbsOrigin.X);
                player.PrintToConsole("Y: " + player.PlayerPawn.Value.AbsOrigin.Y);
                player.PrintToConsole("Z: " + player.PlayerPawn.Value.AbsOrigin.Z);
            });

            AddCommand("skin", "sikesrem", (player, info) =>
            {
                if (player == null) return;
                var weapon = player.PlayerPawn.Value.WeaponServices.ActiveWeapon;
                player.PrintToChatWithPrefix(weapon.Value.DesignerName);
                weapon.Value.AttributeManager.Item.ItemID = 16384;
                weapon.Value.AttributeManager.Item.ItemIDLow = 16384 & 0xFFFFFFFF;
                weapon.Value.AttributeManager.Item.ItemIDHigh = weapon.Value.AttributeManager.Item.ItemIDLow >> 32;
                weapon.Value.FallbackPaintKit = 38;
                weapon.Value.FallbackWear = 0.0001f;
                weapon.Value.FallbackSeed = 0;
            });

            var services = new ServiceCollection();
            services.AddAllTypes<CSurfModule>(ServiceLifetime.Singleton);
            _serviceProvider = services.BuildServiceProvider();
            _serviceProvider.InstanciateStartupScripts();

            RegisterEventHandler<EventServerPreShutdown>((@event, eventInfo) => { Console.Write("#### EventServerPreShutdown fired ####"); return HookResult.Continue; });

            InitializeListener();
        }

        public CSurfPlayer GetSurfPlayerBySteamId(ulong steamId)
        {
            return _databaseService.GetCache().RegisteredSurfPlayers.FirstOrDefault(x => x.SteamId == steamId);
        }

        public List<CSurfPlayer> GetSurfPlayerList()
        {
            return _databaseService.GetCache().RegisteredSurfPlayers;
        }

        public CSurfDatabaseService GetDatabaseService()
        {
            return _databaseService;
        }

        public override void Unload(bool hotReload)
        {
            _databaseService.UpdateDatabase(_databaseService.GetCache().RegisteredSurfPlayers, "registered_players");
        }

        [GameEventHandler]
        public HookResult OnServerShutdown(EventServerShutdown @event, GameEventInfo info)
        {
            if(_databaseService == null) return HookResult.Continue;
            _databaseService.UpdateDatabase(_databaseService.GetCache().RegisteredSurfPlayers, "registered_players");
            
            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            CCSPlayerController playerController = @event.Userid;
            if (playerController == null || !playerController.IsValid || playerController.IsBot) return HookResult.Continue;
            if (GetSurfPlayerList().Exists(x => x.SteamId == playerController.SteamID))
            {
                playerController.PrintToChatWithPrefix("Welcome back " + playerController.PlayerName);
            }
            else
            {
                GetSurfPlayerList().Add(new CSurfPlayer(playerController.SteamID));
                playerController.PrintToChatWithPrefix("Thats your first time here " + playerController.PlayerName + " :) Have fun!");
            };

            CSurfPlayer surfPlayer = GetSurfPlayerBySteamId(playerController.SteamID);

            return HookResult.Continue;
        }

        private void InitializeListener()
        {
        }
    }
}