using CounterStrikeSharp.API.Core;
using System.Runtime.CompilerServices;

namespace CSurf.Helper
{
    public static class PlayerHelper
    {
        public static void PrintToChatWithPrefix(this CCSPlayerController playerController, string message)
        {
            playerController.PrintToChat($"\b[\fCSurf\b] {message}");
        }
    }

    public enum SurfMapState
    {
           FINISHED,
           STARTED,
           IDLE
    }
}
