using Mlie;
using UnityEngine;
using Verse;

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Settings;

internal class Mod_Laser_Drill : Mod
{
    public static ModSettings_LaserDrill Settings;

    public static string currentVersion;

    public Mod_Laser_Drill(ModContentPack content) : base(content)
    {
        Settings = GetSettings<ModSettings_LaserDrill>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override string SettingsCategory()
    {
        return "ED-Laser Drill";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.DoSettingsWindowContents(inRect);
    }
}