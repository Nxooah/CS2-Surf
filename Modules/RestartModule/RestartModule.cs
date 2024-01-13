using System.Numerics;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CSurf.Helper;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace CSurf.Modules.RestartModule;

public class RestartModule : CSurfModule
{
    public RestartModule()
    {
        CSurfPlugin.Instance.AddCommandListener("r", OnRestartCommand);
    }

    private HookResult OnRestartCommand(CCSPlayerController player, CommandInfo info)
    {
        try
        {
            if (player == null || !player.PawnIsAlive) return HookResult.Continue;
            var mapModule = MapModule.MapModule.getCurrentMap();
            var center = CalculateCenterFromToVectors(mapModule.StartZone.Item1, mapModule.StartZone.Item2);
            Vector vector = new(center.X, center.Y, center.Z);
            if (player.PlayerPawn.Value.AbsOrigin != null) 
                player.PlayerPawn.Value.AbsOrigin.X = vector.X;
                player.PlayerPawn.Value.AbsOrigin.Y = vector.Y;
                player.PlayerPawn.Value.AbsOrigin.Z = vector.Z;

                player.PlayerPawn.Value.Teleport(new Vector(vector.X, vector.Y, vector.Z), new QAngle(0, 0, 0), new Vector(0, 0, 0)); 
                
            player.PrintToChatWithPrefix("Respawned!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return HookResult.Stop;
    }

    public Vector3 CalculateCenterFromToVectors(Vector3 v1, Vector3 v2)
    {
        var mins = new Vector3(Math.Min(v1.X, v2.X), Math.Min(v1.Y, v2.Y), Math.Min(v1.Z, v2.Z));
        var maxs = new Vector3(Math.Max(v1.X, v2.X), Math.Max(v1.Y, v2.Y), Math.Max(v1.Z, v2.Z));

        var center = new Vector3((mins.X + maxs.X) / 2, (mins.Y + maxs.Y) / 2, (mins.Z + maxs.Z) / 2);

        return center;
    }
}