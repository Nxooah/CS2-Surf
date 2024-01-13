using System.Net.Mail;
using System.Runtime.InteropServices.JavaScript;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CSurf.Helper;
using CSurf.Models;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace CSurf.Modules
{
    public class TimerModule : CSurfModule
    {

        private string _currentServerHeadColor = "#000000";
        private string _currentLastMessage = $"➠ 𝘿𝙞𝙨𝙘𝙤𝙧𝙙: discord.gg/csurf";

        public TimerModule()
        {
            CSurfPlugin.Instance.RegisterListener<Listeners.OnTick>(OnTick);
            CSurfPlugin.Instance.AddTimer(1f, () =>
            {
                Random random = new Random();
                _currentServerHeadColor = String.Format("#{0:X6}", random.Next(0x1000000));
            }, TimerFlags.REPEAT);
            
            CSurfPlugin.Instance.AddTimer(15f, () =>
            {
                switch (_currentLastMessage)
                {
                    case "➠ 𝘿𝙞𝙨𝙘𝙤𝙧𝙙: discord.gg/csurf" :
                        _currentLastMessage = $"➠ 𝙍𝙖𝙣𝙠: ";
                        break;
                    default:
                        _currentLastMessage = $"➠ 𝘿𝙞𝙨𝙘𝙤𝙧𝙙: discord.gg/csurf";
                        break;
                }
            }, TimerFlags.REPEAT);
        }

        private void OnTick()
        {
            var players = Utilities.GetPlayers()
                .Where(player => player is { IsValid: true, IsBot: false, PawnIsAlive: true });
            
            foreach (var player in players) {
                CSurfPlayer surfPlayer = CSurfPlugin.Instance.GetSurfPlayerBySteamId(player.SteamID);
                if (surfPlayer == null) continue;
                var buttons = player.Buttons;
                var client = player.EntityIndex!.Value.Value;
                if (client == IntPtr.Zero) continue;

                string printHtml = "";
                printHtml += $"<font style='align-items: left;' color='{_currentServerHeadColor}' size='4'>ＣＳｕｒｆ - Ｓｙｓｔｅｍ</font><br>"; //Full With
                printHtml += $"➠ 𝙎𝙥𝙚𝙚𝙙: {Math.Round(player.PlayerPawn.Value.AbsVelocity.Length2D())} u/s<br>";
                switch (surfPlayer.SurfMapState)
                {
                    case SurfMapState.IDLE:
                        printHtml += "➠ 𝙏𝙞𝙢𝙚𝙧: <font style='color='#F2D94E'>00:00:00</font><br>";
                        break;
                    case SurfMapState.STARTED:
                        printHtml += $"➠ 𝙏𝙞𝙢𝙚𝙧: <font style='color='#2E9F65'>{FormatTimeSpan(TimeSpan.FromMilliseconds(GetCurrentTime(surfPlayer.CurrentMapStartTime)))}</font><br>";
                        break;
                    case SurfMapState.FINISHED:
                        printHtml += surfPlayer.BetterThenLastRun ? $"➠ 𝙏𝙞𝙢𝙚𝙧: <font style='color='#2E9F65'>{FormatTimeSpan(TimeSpan.FromMilliseconds(surfPlayer.CurrentMapFinishedTime))}</font><br>" :
                                $"➠ 𝙏𝙞𝙢𝙚𝙧: <font style='color='#bd000d'>{FormatTimeSpan(TimeSpan.FromMilliseconds(surfPlayer.CurrentMapFinishedTime))}</font><br>";
                        break;
                }

                if (_currentLastMessage.Contains("➠ 𝙍𝙖𝙣𝙠: "))
                {
                    var currentRank = GetCurrentRank(surfPlayer);
                    printHtml += _currentLastMessage = $"➠ 𝙍𝙖𝙣𝙠: {currentRank[0]}/{currentRank[1]}"; //Math Sans
                }
                else
                {
                    printHtml += _currentLastMessage; //Math Sans
                }
              
                player.PrintToCenterHtml(printHtml);
            }
        }

        public long GetCurrentTime(long unixTimestamp)
        {
            return CalculateTime(unixTimestamp);
        }
        
        public static long CalculateTime(long unixTimestampMilliseconds)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestampMilliseconds);
            DateTime dateTime = dateTimeOffset.UtcDateTime;

            TimeSpan timeDifference = DateTime.UtcNow - dateTime;            
            return (long) timeDifference.TotalMilliseconds;
        }
        
        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return $"{(int)timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}:{timeSpan.Milliseconds:D2}";
        }

        private int[] GetCurrentRank(CSurfPlayer surfPlayer)
        {
            var playerWhoFinished = CSurfPlugin.Instance.GetSurfPlayerList().Where(x => x.SurfMapDatas.Exists(x => x.MapName == MapModule.MapModule.getCurrentMapName())).ToList();

            if(!playerWhoFinished.Contains(surfPlayer)) return new int[2] { -1, playerWhoFinished.Count() };
            var sorted = playerWhoFinished.OrderBy(x => x.SurfMapDatas.Find(x => x.MapName == MapModule.MapModule.getCurrentMapName()).BestTime).ToList();

            return new int[2] { sorted.IndexOf(surfPlayer) + 1, playerWhoFinished.Count() };
        }
    }
}
