using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CSurf.Helper;
using CSurf.Models;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CSurf.Modules.MapModule
{
    public class KnifeModule : CSurfModule
    {

        private static readonly Dictionary<string, string> knifeTypes = new()
    {
        { "m9", "weapon_knife_m9_bayonet" },
        { "karambit", "weapon_knife_karambit" },
        { "bayonet", "weapon_bayonet" },
        { "bowie", "weapon_knife_survival_bowie" },
        { "butterfly", "weapon_knife_butterfly" },
        { "falchion", "weapon_knife_falchion" },
        { "flip", "weapon_knife_flip" },
        { "gut", "weapon_knife_gut" },
        { "tactical", "weapon_knife_tactical" },
        { "shadow", "weapon_knife_push" },
        { "navaja", "weapon_knife_gypsy_jackknife" },
        { "stiletto", "weapon_knife_stiletto" },
        { "talon", "weapon_knife_widowmaker" },
        { "ursus", "weapon_knife_ursus" },
        { "css", "weapon_knife_css" },
        { "paracord", "weapon_knife_cord" },
        { "survival", "weapon_knife_canis" },
        { "nomad", "weapon_knife_outdoor" },
        { "skeleton", "weapon_knife_skeleton" },
        { "default", "weapon_knife" }
    };

        public KnifeModule()
        {
            CSurfPlugin.Instance.RegisterListener<Listeners.OnEntitySpawned>(OnEntitySpawned);
            CSurfPlugin.Instance.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            CSurfPlugin.Instance.AddCommandListener("knife", OnKnifeCommand);
        }

        private HookResult OnKnifeCommand(CCSPlayerController player, CommandInfo info)
        {
            try
            {
                if (player == null || !player.PawnIsAlive) return HookResult.Continue;
                if (info.ArgCount == 1)
                {
                    player.PrintToChatWithPrefix("Please use !knife <knifeType> <?skinId>");
                    return HookResult.Stop;
                }

                if (!knifeTypes.ContainsKey(info.ArgByIndex(1)))
                {
                    player.PrintToChatWithPrefix($"The knife {ChatColors.Red + info.ArgByIndex(1) + ChatColors.White} doesnt exists.");
                    return HookResult.Stop;
                }

                var isNumeric = int.TryParse(info.ArgByIndex(2), out int n);
                if (isNumeric)
                {
                    CSurfPlugin.Instance.GetSurfPlayerBySteamId(player.SteamID).ChoosenKnifePaintKit = n;
                }

                if (player.Pawn.Value.WeaponServices != null)
                {
                    if (player.Pawn.Value.WeaponServices.MyWeapons.Count != 0)
                    {
                        Console.WriteLine($"1: {player.Pawn} \n");
                        Console.WriteLine($"2: {player.Pawn.Value} \n");
                        Console.WriteLine($"3: {player.Pawn.Value.WeaponServices} \n");
                        Console.WriteLine($"3: {player.Pawn.Value.WeaponServices.MyWeapons} \n");
                
                        var knifePawn = player.Pawn?.Value.WeaponServices?.MyWeapons?.FirstOrDefault(x => x != null && x.Value != null && x.Value.DesignerName != null && x.Value.DesignerName.Contains("knife"));
                        if (knifePawn != null && !knifePawn.IsValid)
                        {
                            knifePawn.Value.Remove();
                        }
                    }   
                }

                if (CSurfPlugin.Instance.GetSurfPlayerBySteamId(player.SteamID).CurrentCustomKnifeRef != null)
                {
                    CSurfPlugin.Instance.GetSurfPlayerBySteamId(player.SteamID).CurrentCustomKnifeRef.Remove();
                }

                player.GiveNamedItem(knifeTypes.GetValueOrDefault(info.ArgByIndex(1)));
                CSurfPlugin.Instance.GetSurfPlayerBySteamId(player.SteamID).ChoosenKnife = info.ArgByIndex(1);

                return HookResult.Stop;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            CCSPlayerController playerController = @event.Userid;
            if (playerController == null || !playerController.IsValid) return HookResult.Continue;

            CSurfPlayer surfPlayer = CSurfPlugin.Instance.GetSurfPlayerBySteamId(playerController.SteamID);
            if (surfPlayer == null) return HookResult.Continue;

            CSurfPlugin.Instance.AddTimer(0.1f, () =>
            {
                playerController.RemoveWeapons();
                playerController.GiveNamedItem(knifeTypes.GetValueOrDefault(surfPlayer.ChoosenKnife));
            });

            return HookResult.Continue;
        }

        private void OnEntitySpawned(CEntityInstance entity)
        {
            var weapon = new CBasePlayerWeapon(entity.Handle);
            if (!knifeTypes.ContainsValue(entity.DesignerName)) return;

            Server.NextFrame(() =>
            {
                try
                {
                    if (!weapon.IsValid) return;
                    if (weapon.OwnerEntity.Value == null) return;
                    if (!weapon.OwnerEntity.Value.EntityIndex.HasValue) return;

                    int weaponOwner = (int)weapon.OwnerEntity.Value.EntityIndex.Value.Value;
                    var pawn = new CBasePlayerPawn(NativeAPI.GetEntityFromIndex(weaponOwner));

                    if (!pawn.IsValid) return;

                    var playerIndex = (int)pawn.Controller.Value.EntityIndex!.Value.Value;
                    var player = Utilities.GetPlayerFromIndex(playerIndex);

                    if (player == null || !player.IsValid || player.IsBot) return;
                    weapon.AttributeManager.Item.ItemID = 16384;
                    weapon.AttributeManager.Item.ItemIDLow = 16384 & 0xFFFFFFFF;
                    weapon.AttributeManager.Item.ItemIDHigh = weapon.AttributeManager.Item.ItemIDLow >> 32;
                    weapon.FallbackPaintKit = CSurfPlugin.Instance.GetSurfPlayerBySteamId(player.SteamID).ChoosenKnifePaintKit;
                    weapon.FallbackWear = 0.0001f;
                    weapon.FallbackSeed = 0;

                    CSurfPlugin.Instance.GetSurfPlayerBySteamId(player.SteamID).CurrentCustomKnifeRef = weapon;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
        }
    }
}
