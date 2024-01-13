using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CSurf.Helper;
using CSurf.Models;
using System.Numerics;
using System.Runtime.Intrinsics;
using CounterStrikeSharp.API.Modules.Utils;
using static CounterStrikeSharp.API.Core.Listeners;

namespace CSurf.Modules.MapModule;

public class MapModule : CSurfModule
{
    private List<Map> _surfMaps;
    private static Map _currentMap;

    public MapModule()
    {
        CSurfPlugin.Instance.RegisterListener<Listeners.OnTick>(OnTick);

        _surfMaps = new List<Map>();

        Map utopia = new(
            name: "utopia",
            startZone: Tuple.Create(new Vector3(-13775.068359f, 514.466675f, 13364.705078f), new Vector3(-14283.035156f, -509.662354f, 12799.031f)),
            endZone: Tuple.Create(new Vector3(-13854.310547f, -758.869995f, -6147.187500f), new Vector3(-14328.564453f, 812.292114f, -5909.262695f))
        );
        _surfMaps.Add(utopia);

        _currentMap = utopia;
    }

    private void OnTick()
    {
        var players = Utilities.GetPlayers()
            .Where(player => player is { IsValid: true, IsBot: false, PawnIsAlive: true });

        foreach (var player in players)
        {
            var client = player.EntityIndex!.Value.Value;
            if (client == IntPtr.Zero) continue;

            CSurfPlayer surfPlayer = CSurfPlugin.Instance.GetSurfPlayerBySteamId(player.SteamID);
            if (surfPlayer == null) continue;

            Vector3 playerLocation = new Vector3(player.PlayerPawn.Value.AbsOrigin.X,
                player.PlayerPawn.Value.AbsOrigin.Y, player.PlayerPawn.Value.AbsOrigin.Z);

            if (IsInTwoVectors(_currentMap.StartZone.Item1, _currentMap.StartZone.Item2, playerLocation))
            {
                if (surfPlayer.SurfMapState != SurfMapState.IDLE)
                {
                    surfPlayer.SurfMapState = SurfMapState.IDLE;
                }
            }
            else
            {
                if (surfPlayer.SurfMapState == SurfMapState.IDLE)
                {
                    surfPlayer.SurfMapState = SurfMapState.STARTED;
                    surfPlayer.CurrentMapStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }

            if (IsInTwoVectors(_currentMap.EndZone.Item1, _currentMap.EndZone.Item2, playerLocation))
            {
                if (surfPlayer.SurfMapState != SurfMapState.FINISHED &&
                    surfPlayer.SurfMapState == SurfMapState.STARTED)
                {
                    surfPlayer.CurrentMapFinishedTime = TimerModule.CalculateTime(surfPlayer.CurrentMapStartTime);
                    string finishTime = TimerModule.FormatTimeSpan(TimeSpan.FromMilliseconds(surfPlayer.CurrentMapFinishedTime));
                    Server.PrintToChatAll($"\b[\fCSurf\b] {ChatColors.Gold}{player.PlayerName} {ChatColors.White}finished in {ChatColors.Gold}{finishTime}");
                    surfPlayer.SurfMapState = SurfMapState.FINISHED;

                    CSurfMapData surfMapData;
                    if (surfPlayer.SurfMapDatas.Count == 0)
                    {
                        surfMapData = new CSurfMapData { MapName = _currentMap.Name, BestTime = surfPlayer.CurrentMapFinishedTime};
                        surfPlayer.SurfMapDatas.Add(surfMapData);
                        player.PrintToChatWithPrefix($"You finished {ChatColors.Blue}{_currentMap.Name}{ChatColors.White} for first time within a time of {ChatColors.Gold}{finishTime}");
                    }
                    else
                    {
                        surfMapData = surfPlayer.SurfMapDatas.FirstOrDefault(x => x.MapName == _currentMap.Name);
                        if (surfMapData != null)
                        {
                            if(surfPlayer.CurrentMapFinishedTime >= surfMapData.BestTime)
                            {
                                surfPlayer.BetterThenLastRun = false;
                                player.PrintToChatWithPrefix($"You finished {ChatColors.Blue}{_currentMap.Name}{ChatColors.White} within a time of {ChatColors.Gold}{finishTime}");
                            } else
                            {
                                surfMapData.BestTime = surfPlayer.CurrentMapFinishedTime;
                                surfPlayer.BetterThenLastRun = true;
                                player.PrintToChatWithPrefix($"You finished {ChatColors.Blue}{_currentMap.Name}{ChatColors.White} with new best time of {ChatColors.Gold}{finishTime}");
                            }
                        }
                        else
                        {
                            surfMapData = new CSurfMapData { MapName = _currentMap.Name, BestTime = surfPlayer.CurrentMapFinishedTime };
                            surfPlayer.SurfMapDatas.Add(surfMapData);
                            surfPlayer.BetterThenLastRun = true;
                            player.PrintToChatWithPrefix($"You finished {ChatColors.Blue}{_currentMap.Name}{ChatColors.White} for first time within a time of {ChatColors.Gold}{finishTime}");


                        }
                    }

                    //Removing Element from Array: surfPlayer.SurfMapDatas = surfPlayer.SurfMapDatas.Where((source, index) => source.MapName != "utopia").ToArray(); 
                }
            }
        }
    }

    public bool IsInTwoVectors(Vector3 pos1, Vector3 pos2, Vector3 check)
    {
        float minX = Math.Min(pos1.X, pos2.X);
        float maxX = Math.Max(pos1.X, pos2.X);
        float minY = Math.Min(pos1.Y, pos2.Y);
        float maxY = Math.Max(pos1.Y, pos2.Y);
        float minZ = Math.Min(pos1.Z, pos2.Z);
        float maxZ = Math.Max(pos1.Z, pos2.Z);
        return check.X >= minX && check.X <= maxX && check.Y >= minY && check.Y <= maxY
            && check.Z >= minZ && check.Z <= maxZ;
    }

    public static string getCurrentMapName()
    {
        return _currentMap.Name;
    }

    public static Map getCurrentMap()
    {
        return _currentMap;
    }
}