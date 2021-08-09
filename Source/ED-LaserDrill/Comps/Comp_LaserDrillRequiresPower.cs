using System;
using Jaxxa.EnhancedDevelopment.Core.Comp.Interface;
using RimWorld;
using Verse;

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Comps
{
    // Token: 0x02000006 RID: 6
    internal class Comp_LaserDrillRequiresPower : ThingComp, IRequiresShipResources
    {
        // Token: 0x0400000A RID: 10
        private readonly int m_RequiredEnergy = 6000;

        // Token: 0x04000009 RID: 9
        private CompPowerTrader m_PowerComp;

        // Token: 0x06000015 RID: 21 RVA: 0x00002480 File Offset: 0x00000680
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

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000016 RID: 22 RVA: 0x000024EF File Offset: 0x000006EF
        bool IRequiresShipResources.Satisfied => HasEnoughEnergy(out _);

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x06000017 RID: 23 RVA: 0x000024F8 File Offset: 0x000006F8
        string IRequiresShipResources.StatusString
        {
            get
            {
                if (HasEnoughEnergy(out var currentEnergy))
                {
                    return "Sufficient Power for Drill Activation, ready to use 6,000 Wd.";
                }

                return "Insufficient Power stored for Drill Activation, needs 6,000 Wd. Currently has " +
                       Math.Floor(currentEnergy) + " Wd.";
            }
        }

        // Token: 0x06000013 RID: 19 RVA: 0x00002405 File Offset: 0x00000605
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            m_PowerComp = parent.TryGetComp<CompPowerTrader>();
        }

        // Token: 0x06000014 RID: 20 RVA: 0x00002420 File Offset: 0x00000620
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
                currentEnergy = (float) num;
            }

            var num2 = num;
            float num3 = m_RequiredEnergy;
            return (num2.GetValueOrDefault() >= num3) & (num2 != null);
        }
    }
}