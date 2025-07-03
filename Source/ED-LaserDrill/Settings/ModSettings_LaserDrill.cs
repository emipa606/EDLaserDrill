using UnityEngine;
using Verse;

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Settings;

internal class ModSettings_LaserDrill : ModSettings
{
    public bool AllowSimultaneousDrilling;
    public bool RemoveAfterOneUse = true;

    public int RequiredScanningTimeDays = 10;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref RequiredScanningTimeDays, "RequiredScanningTimeDays", 10, true);
        Scribe_Values.Look(ref AllowSimultaneousDrilling, "AllowSimultaneousDrilling", false, true);
        Scribe_Values.Look(ref RemoveAfterOneUse, "RemoveAfterOneUse", true, true);
    }

    public void DoSettingsWindowContents(Rect canvas)
    {
        var listingStandard = new Listing_Standard { ColumnWidth = 250f };
        listingStandard.Begin(canvas);
        listingStandard.GapLine();
        listingStandard.Label("EDL.scanningdays".Translate(RequiredScanningTimeDays));
        listingStandard.Gap();
        var listingStandard2 = new Listing_Standard();
        listingStandard2.Begin(listingStandard.GetRect(30f));
        listingStandard2.ColumnWidth = 70f;
        listingStandard2.IntAdjuster(ref RequiredScanningTimeDays, 1, 1);
        listingStandard2.NewColumn();
        listingStandard2.IntSetter(ref RequiredScanningTimeDays, 10, "EDL.default".Translate());
        listingStandard2.End();
        listingStandard.GapLine();
        listingStandard.CheckboxLabeled("EDL.allowsimultaneous".Translate(), ref AllowSimultaneousDrilling,
            "EDL.allowsimultaneous.description".Translate());
        listingStandard.CheckboxLabeled("EDL.removeafteroneuse".Translate(), ref RemoveAfterOneUse,
            "EDL.removeafteroneuse.description".Translate());
        listingStandard.GapLine();
        if (Mod_Laser_Drill.currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("EDL.CurrentModVersion".Translate(Mod_Laser_Drill.currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
    }
}