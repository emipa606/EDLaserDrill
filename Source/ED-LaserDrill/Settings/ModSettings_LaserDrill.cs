using UnityEngine;
using Verse;

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Settings
{
    // Token: 0x02000003 RID: 3
    internal class ModSettings_LaserDrill : ModSettings
    {
        // Token: 0x04000003 RID: 3
        public bool AllowSimultaneousDrilling;

        // Token: 0x04000002 RID: 2
        public int RequiredScanningTimeDays = 10;

        // Token: 0x06000004 RID: 4 RVA: 0x00002078 File Offset: 0x00000278
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref RequiredScanningTimeDays, "RequiredScanningTimeDays", 10, true);
            Scribe_Values.Look(ref AllowSimultaneousDrilling, "AllowSimultaneousDrilling", false, true);
        }

        // Token: 0x06000005 RID: 5 RVA: 0x000020A8 File Offset: 0x000002A8
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
            listing_Standard.End();
        }
    }
}