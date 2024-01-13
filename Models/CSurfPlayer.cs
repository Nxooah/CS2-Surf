using CounterStrikeSharp.API.Core;
using CSurf.Helper;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CSurf.Models
{
    public class CSurfPlayer
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ulong SteamId { get; set; }
        [BsonIgnore]
        public SurfMapState SurfMapState { get; set; } = SurfMapState.IDLE;
        [BsonIgnore]
        public long CurrentMapStartTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        [BsonIgnore]
        public long CurrentMapFinishedTime { get; set; } = 0;
        [BsonIgnore]
        public bool BetterThenLastRun { get; set; }
        public List<CSurfMapData> SurfMapDatas { get; set; } = new List<CSurfMapData>();
        public string ChoosenKnife { get; set; } = "default";
        public int ChoosenKnifePaintKit { get; set; } = 0;
        [BsonIgnore]
        public CBasePlayerWeapon CurrentCustomKnifeRef { get; set; }

        public CSurfPlayer(ulong steamId) {
            SteamId = steamId;

        }
        
        public int GetCurrentPlacement(long bestTime, string mapName)
        {
            List<long> bestTimes = SurfMapDatas
                .Where(mapData => mapData.MapName == mapName)
                .Select(mapData => mapData.BestTime)
                .ToList();
            
            int currentPlacement = GetCurrentPlacement(bestTimes, bestTime);

            return currentPlacement;
        }

        static int GetCurrentPlacement(List<long> list, long valueToFind)
        {
            var sortedList = list.OrderBy(value => value).ToList();
            int currentPlacement = sortedList.FindIndex(val => val == valueToFind);
            
            return currentPlacement != -1 ? currentPlacement + 1 : -1;
        }
    }
}
