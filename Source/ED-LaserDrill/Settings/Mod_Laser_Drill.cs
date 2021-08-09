using UnityEngine;
using Verse;

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Settings
{
    // Token: 0x02000002 RID: 2
    internal class Mod_Laser_Drill : Mod
    {
        // Token: 0x04000001 RID: 1
        public static ModSettings_LaserDrill Settings;

        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public Mod_Laser_Drill(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ModSettings_LaserDrill>();
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002064 File Offset: 0x00000264
        public override string SettingsCategory()
        {
            return "ED-Laser Drill";
        }

        // Token: 0x06000003 RID: 3 RVA: 0x0000206B File Offset: 0x0000026B
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }
}