using DV;
using DV.Logic.Job;
using DV.ThingTypes;
using DV.Utils;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;

namespace dvSlugSpawns
{
    [HarmonyPatch(typeof(StationProceduralJobsController), "TryToGenerateJobs")]
    public static class StationProceduralJobsController_Patch
    {
        private static TrainCarLivery? slugLivery = null;
        private static Dictionary<string, List<RailTrack>> slugSpawnTracks = new Dictionary<string, List<RailTrack>>();

        public static void Prefix(StationProceduralJobsController __instance)
        {
            Station station = __instance.stationController.logicStation;
            if (CsvConfig.slugSpawns.ContainsKey(station.ID))
                __instance.StartCoroutine(spawnSlugCoro(station));
        }

        private static IEnumerator spawnSlugCoro(Station station)
        {
            GarageType_v2? slugGarage = Globals.G.Types.Garage_to_v2[Garage.DE6_Slug];
            if (slugGarage == null) {
                Main.ModEntry.Logger.Error("No Slug garage found, broken map?");
                yield break;
            }
            bool garageUnlocked = SingletonBehaviour<LicenseManager>.Instance.IsGarageUnlocked(slugGarage);
            if (!garageUnlocked && !Main.Settings.ForceSpawn) {
                Main.ModEntry.Logger.Log("Slug garage still locked, not spawning");
                yield break;
            }
            if (!CacheLivery()) {
                yield break;
            }
            yield return null;

            string stationId = station.ID;
            List<string> trackNames;
            if (!CsvConfig.slugSpawns.TryGetValue(stationId, out trackNames)) {
                yield break;
            }
            foreach (string trackName in trackNames) {
                if (!slugSpawnTracks.ContainsKey(stationId)) {
                    CacheStationTracks(stationId, trackNames);
                }
                TrySpawnOnTrack(stationId, trackName, CsvConfig.tryOccupiedTracks.ContainsKey(trackName));
                yield return null;
            }
        }

        private static void TrySpawnOnTrack(string stationId, string trackName, bool tryOccupied)
        {
            RailTrack? spawnTrack = TryFindStationTrack(stationId, trackName);
            if (spawnTrack == null) {
                Main.ModEntry.Logger.Error($"Unknown track {trackName} at station {stationId}, skipping");
                return;
            }
            List<Car> carsFullyOnTrack = spawnTrack.logicTrack.GetCarsFullyOnTrack();
            if (!tryOccupied && !Main.Settings.ForceOccupied && carsFullyOnTrack.Count > 0) {
                return;
            }
            List<TrainCarLivery> trainCarTypes = new(){ slugLivery };
            List<TrainCar> spawnedCars = SingletonBehaviour<CarSpawner>.Instance.SpawnCarTypesOnTrack(trainCarTypes, null, spawnTrack, true, true);
            if (spawnedCars != null && spawnedCars.Count > 0) {
                Main.ModEntry.Logger.Log($"Slug spawned at {spawnTrack} (forced:{Main.Settings.ForceSpawn})");
            }
        }

        private static RailTrack? TryFindStationTrack(string stationId, string trackName)
        {
            if (slugSpawnTracks.TryGetValue(stationId, out List<RailTrack> trackList)) {
                foreach (RailTrack railTrack in trackList) {
                    if (railTrack.logicTrack.ID.FullDisplayID == trackName) {
                        return railTrack;
                    }
                }
            }

            return null;
        }

        private static void CacheStationTracks(string stationId, List<string> cfgTrackNames)
        {
            if (!slugSpawnTracks.ContainsKey(stationId)) {
                slugSpawnTracks.Add(stationId, new List<RailTrack>());
            }
            foreach (RailTrack railTrack in SingletonBehaviour<RailTrackRegistry>.Instance.AllTracks) {
                foreach (string cfgTrackName in cfgTrackNames) {
                    if (railTrack.logicTrack.ID.FullDisplayID == cfgTrackName) {
                        if (slugSpawnTracks.TryGetValue(stationId, out List<RailTrack> trackList)) {
                            Main.ModEntry.Logger.Log($"Caching station track stationId:{stationId} trackName:{cfgTrackName}");
                            trackList.Add(railTrack);
                        }
                        break;
                    }
                }
            }
        }

        private static bool CacheLivery()
        {
            if (slugLivery != null) {
                return true;
            }
            foreach (TrainCarLivery livery in Globals.G.Types.Liveries) {
                if (livery.v1 == TrainCarType.LocoDE6Slug) {
                    Main.ModEntry.Logger.Log("Slug livery cached");
                    slugLivery = livery;
                    return true;
                }
            }
            Main.ModEntry.Logger.Error("Slug livery not found, broken game?");
            return false;
        }
    }
}
