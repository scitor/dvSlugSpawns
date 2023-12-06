using UnityModManagerNet;

namespace dvSlugSpawns
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Force spawn (ignore garage unlock)")]
        public bool ForceSpawn = false;

        public override void Save(UnityModManager.ModEntry modEntry) {
            Save(this, modEntry);
        }

        public void OnChange() { }
    }
}
