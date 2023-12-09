using UnityModManagerNet;

namespace dvSlugSpawns
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Ignore garage unlock (force spawn)")]
        public bool ForceSpawn = false;

        [Draw("Always spawn on occupied tracks (caution)")]
        public bool ForceOccupied = false;

        public override void Save(UnityModManager.ModEntry modEntry) {
            Save(this, modEntry);
        }

        public void OnChange() { }
    }
}
