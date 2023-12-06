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

        public static void Prefix(StationProceduralJobsController __instance)
        {
            if (__instance.stationController.logicStation.ID == "SM")
                __instance.StartCoroutine(spawnSlugCoro());
        }

        private static IEnumerator spawnSlugCoro()
        {
            GarageType_v2? slugGarage = Globals.G.Types.Garage_to_v2[Garage.DE6_Slug];
            if (slugGarage == null) {
                Main.ModEntry.Logger.Log("No Slug garage found");
                yield break;
            }
            bool garageUnlocked = SingletonBehaviour<LicenseManager>.Instance.IsGarageUnlocked(slugGarage);
            if (!garageUnlocked && !Main.Settings.ForceSpawn) {
                Main.ModEntry.Logger.Log("Slug garage locked, not spawning");
                yield break;
            }
            if (slugLivery == null) {
                foreach (TrainCarLivery livery in Globals.G.Types.Liveries) {
                    if (livery.v1 == TrainCarType.LocoDE6Slug) {
                        Main.ModEntry.Logger.Log("Slug livery cached");
                        slugLivery = livery;
                    }
                }
            }
            if (slugLivery == null) {
                Main.ModEntry.Logger.Log("Slug livery not found");
                yield break;
            }
            yield return null;
            List<TrainCarLivery> trainCarTypes = new List<TrainCarLivery> { slugLivery };
            string[] trackNames = { "#Y-#S436#T" };
            foreach (string trackName in trackNames) {
                RailTrack? locoSpawnTrack = null;
                foreach (RailTrack railTrack in SingletonBehaviour<RailTrackRegistry>.Instance.AllTracks) {
                    if (railTrack.logicTrack != null && railTrack.logicTrack.ID.FullDisplayID == trackName) {
                        locoSpawnTrack = (RailTrack)railTrack;
                        break;
                    }
                }
                if (locoSpawnTrack != null) {
                    List<Car> carsFullyOnTrack = locoSpawnTrack.logicTrack.GetCarsFullyOnTrack();
                    if (carsFullyOnTrack.Count == 0) {
                        List<TrainCar> spawnedCars = SingletonBehaviour<CarSpawner>.Instance.SpawnCarTypesOnTrack(trainCarTypes, null, locoSpawnTrack, true, true);
                        if (spawnedCars != null && spawnedCars.Count > 0){
                            Main.ModEntry.Logger.Log($"Slug spawned at {locoSpawnTrack} (forced:{Main.Settings.ForceSpawn})");
                        }
                    }
                }
                yield return null;
            }
        }
    }
}
