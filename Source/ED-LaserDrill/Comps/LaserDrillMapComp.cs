using System.Collections.Generic;
using Jaxxa.EnhancedDevelopment.LaserDrill.Settings;
using Verse;

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Comps
{
    // Token: 0x02000007 RID: 7
    internal class LaserDrillMapComp : MapComponent
    {
        // Token: 0x0400000C RID: 12
        public const int CHECK_INTERVAL = 1000;

        // Token: 0x0400000D RID: 13
        private readonly List<Comp_LaserDrill> comps = new List<Comp_LaserDrill>();

        // Token: 0x0400000B RID: 11
        private Map m_Map;

        // Token: 0x06000019 RID: 25 RVA: 0x00002553 File Offset: 0x00000753
        public LaserDrillMapComp(Map map) : base(map)
        {
            m_Map = map;
        }

        // Token: 0x0600001A RID: 26 RVA: 0x00002570 File Offset: 0x00000770
        public override void MapComponentTick()
        {
            if (Mod_Laser_Drill.Settings.AllowSimultaneousDrilling || Find.TickManager.TicksGame % 1000 != 0)
            {
                return;
            }

            var isScanning = false;
            foreach (var compLaserDrill in comps)
            {
                if (isScanning)
                {
                    compLaserDrill.StopScanning();
                }

                if (compLaserDrill.IsScanning())
                {
                    isScanning = true;
                }
            }
        }

        // Token: 0x0600001B RID: 27 RVA: 0x000025DC File Offset: 0x000007DC
        public void Register(Comp_LaserDrill c)
        {
            comps.Add(c);
        }

        // Token: 0x0600001C RID: 28 RVA: 0x000025EA File Offset: 0x000007EA
        public void Deregister(Comp_LaserDrill c)
        {
            comps.Remove(c);
        }
    }
}