using System.Collections.Generic;
using Jaxxa.EnhancedDevelopment.LaserDrill.Settings;
using Verse;

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Comps;

internal class LaserDrillMapComp : MapComponent
{
    public const int CHECK_INTERVAL = 1000;

    private readonly List<Comp_LaserDrill> comps = new List<Comp_LaserDrill>();

    private Map m_Map;

    public LaserDrillMapComp(Map map) : base(map)
    {
        m_Map = map;
    }

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

    public void Register(Comp_LaserDrill c)
    {
        comps.Add(c);
    }

    public void Deregister(Comp_LaserDrill c)
    {
        comps.Remove(c);
    }
}