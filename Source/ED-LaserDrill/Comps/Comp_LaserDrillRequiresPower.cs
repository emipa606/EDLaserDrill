using System;
using Jaxxa.EnhancedDevelopment.Core.Comp.Interface;
using RimWorld;
using Verse;

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Comps;

internal class Comp_LaserDrillRequiresPower : ThingComp, IRequiresShipResources
{
    private readonly int m_RequiredEnergy = 6000;

    private CompPowerTrader m_PowerComp;

    public bool UseResources()
    {
        if (!HasEnoughEnergy(out _))
        {
            return false;
        }

        var powerComp = m_PowerComp;
        if (powerComp?.PowerNet == null)
        {
            return false;
        }

        float num = m_RequiredEnergy;
        foreach (var compPowerBattery in powerComp.PowerNet.batteryComps)
        {
            var num2 = Math.Min(num, compPowerBattery.StoredEnergy);
            num -= num2;
            compPowerBattery.DrawPower(num2);
        }

        return true;
    }

    bool IRequiresShipResources.Satisfied => HasEnoughEnergy(out _);

    string IRequiresShipResources.StatusString => HasEnoughEnergy(out var currentEnergy)
        ? "EDL.enoughpower".Translate(m_RequiredEnergy)
        : "EDL.notenoughpower".Translate(m_RequiredEnergy, Math.Floor(currentEnergy));

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        m_PowerComp = parent.TryGetComp<CompPowerTrader>();
    }

    private bool HasEnoughEnergy(out float currentEnergy)
    {
        var powerComp = m_PowerComp;
        currentEnergy = 0;
        float? num;
        if (powerComp == null)
        {
            num = null;
        }
        else
        {
            var powerNet = powerComp.PowerNet;
            num = powerNet != null ? new float?(powerNet.CurrentStoredEnergy()) : null;
        }

        if (num != null)
        {
            currentEnergy = (float)num;
        }

        var num2 = num;
        float num3 = m_RequiredEnergy;
        return (num2.GetValueOrDefault() >= num3) & (num2 != null);
    }
}