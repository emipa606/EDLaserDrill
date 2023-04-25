using UnityEngine;
using Verse;

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Settings;

internal class ModSettings_LaserDrill : ModSettings
{
    public bool AllowSimultaneousDrilling;

    public int RequiredScanningTimeDays = 10;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref RequiredScanningTimeDays, "RequiredScanningTimeDays", 10, true);
        Scribe_Values.Look(ref AllowSimultaneousDrilling, "AllowSimultaneousDrilling", false, true);
    }

    public void DoSettingsWindowContents(Rect canvas)
    {
        var listing_Standard = new Listing_Standard { ColumnWidth = 250f };
        listing_Standard.Begin(canvas);
        listing_Standard.GapLine();
        listing_Standard.Label("EDL.scanningdays".Translate(RequiredScanningTimeDays));
        listing_Standard.Gap();
        var listing_Standard2 = new Listing_Standard();
        listing_Standard2.Begin(listing_Standard.GetRect(30f));
        listing_Standard2.ColumnWidth = 70f;
        listing_Standard2.IntAdjuster(ref RequiredScanningTimeDays, 1, 1);
        listing_Standard2.NewColumn();
        listing_Standard2.IntSetter(ref RequiredScanningTimeDays, 10, "EDL.default".Translate());
        listing_Standard2.End();
        listing_Standard.GapLine();
        listing_Standard.CheckboxLabeled("EDL.allowsimultaneous".Translate(), ref AllowSimultaneousDrilling,
            "EDL.allowsimultaneous.description".Translate());
        listing_Standard.GapLine();
        if (Mod_Laser_Drill.currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("EDL.CurrentModVersion".Translate(Mod_Laser_Drill.currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }
}