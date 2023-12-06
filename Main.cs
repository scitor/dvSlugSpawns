using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;

namespace dvSlugSpawns;

public static class Main
{
    public static Settings Settings { get; private set; } = null!;

    public static UnityModManager.ModEntry ModEntry { get; private set; } = null!;

    // Unity Mod Manage Wiki: https://wiki.nexusmods.com/index.php/Category:Unity_Mod_Manager
    private static bool Load(UnityModManager.ModEntry modEntry)
    {
        Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
        modEntry.OnGUI = OnDrawGUI;
        modEntry.OnSaveGUI = OnSaveGUI;
        modEntry.OnToggle = OnToggle;
        ModEntry = modEntry;
        return true;
    }

    static void OnDrawGUI(UnityModManager.ModEntry entry)
    {
        Settings.Draw(entry);
    }

    static void OnSaveGUI(UnityModManager.ModEntry entry)
    {
        Settings.Save(entry);
    }

    private static bool OnToggle(UnityModManager.ModEntry modEntry, bool active)
    {
        Harmony harmony = new Harmony(modEntry.Info.Id);
        if (active) {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        } else {
            harmony.UnpatchAll(modEntry.Info.Id);
        }
        return true;
    }
}
