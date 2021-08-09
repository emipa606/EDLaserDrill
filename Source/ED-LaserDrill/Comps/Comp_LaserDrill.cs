using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jaxxa.EnhancedDevelopment.Core.Comp.Interface;
using Jaxxa.EnhancedDevelopment.LaserDrill.Settings;
using Jaxxa.EnhancedDevelopment.LaserDrill.Things;
using RimWorld;
using UnityEngine;
using Verse;

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Comps
{
    // Token: 0x02000009 RID: 9
    [StaticConstructorOnStartup]
    internal class Comp_LaserDrill : ThingComp
    {
        // Token: 0x04000010 RID: 16
        private static readonly Texture2D UI_LASER_ACTIVATE = ContentFinder<Texture2D>.Get("UI/Power/SteamGeyser");

        // Token: 0x04000011 RID: 17
        private static readonly Texture2D UI_LASER_ACTIVATEFILL =
            ContentFinder<Texture2D>.Get("UI/Power/RemoveSteamGeyser");

        // Token: 0x0400000E RID: 14
        private int DrillScanningRemainingTicks;

        // Token: 0x04000013 RID: 19
        private CompFlickable m_FlickComp;

        // Token: 0x04000012 RID: 18
        private CompPowerTrader m_PowerComp;

        // Token: 0x04000014 RID: 20
        private IRequiresShipResources m_RequiresShipResourcesComp;

        // Token: 0x0400000F RID: 15
        private CompProperties_LaserDrill Properties;

        // Token: 0x0600001F RID: 31 RVA: 0x00002633 File Offset: 0x00000833
        private bool HasSufficientShipResources()
        {
            return m_RequiresShipResourcesComp.Satisfied;
        }

        // Token: 0x06000020 RID: 32 RVA: 0x00002640 File Offset: 0x00000840
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            m_FlickComp = parent.GetComp<CompFlickable>();
            Properties = props as CompProperties_LaserDrill;
            m_PowerComp = parent.TryGetComp<CompPowerTrader>();
            if (!(parent.GetComps<ThingComp>().FirstOrDefault(x => x is IRequiresShipResources) is
                IRequiresShipResources requiresShipResources))
            {
                Log.Error("Comp_LaserDrill Failed to get Comp With IRequiresShipResources");
            }
            else
            {
                m_RequiresShipResourcesComp = requiresShipResources;
            }

            if (!respawningAfterLoad)
            {
                SetRequiredDrillScanningToDefault();
            }

            parent.Map.GetComponent<LaserDrillMapComp>().Register(this);
        }

        // Token: 0x06000021 RID: 33 RVA: 0x000026F2 File Offset: 0x000008F2
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref DrillScanningRemainingTicks, "DrillScanningRemainingTicks");
        }

        // Token: 0x06000022 RID: 34 RVA: 0x0000270C File Offset: 0x0000090C
        public override void CompTickRare()
        {
            if (IsScanning())
            {
                DrillScanningRemainingTicks -= 250;
            }

            base.CompTickRare();
        }

        // Token: 0x06000023 RID: 35 RVA: 0x00002730 File Offset: 0x00000930
        public override string CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            if (IsScanComplete())
            {
                stringBuilder.AppendLine("Scan complete");
            }
            else if (HasPowerToScan())
            {
                stringBuilder.AppendLine("Scanning in Progress - Remaining: " +
                                         DrillScanningRemainingTicks.ToStringTicksToPeriod());
            }
            else
            {
                stringBuilder.AppendLine("Scanning Paused, Power Offline.");
            }


            stringBuilder.Append(m_RequiresShipResourcesComp.StatusString);
            return stringBuilder.ToString();
        }

        // Token: 0x06000024 RID: 36 RVA: 0x000027A7 File Offset: 0x000009A7
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            yield return new Command_Action
            {
                action = TriggerLaser,
                icon = UI_LASER_ACTIVATE,
                defaultLabel = "Activate Laser",
                defaultDesc = "Activate Laser",
                activateSound = SoundDef.Named("Click")
            };
            yield return new Command_Action
            {
                action = TriggerLaserToFill,
                icon = UI_LASER_ACTIVATEFILL,
                defaultLabel = "Activate Laser Fill",
                defaultDesc = "Activate Laser Fill",
                activateSound = SoundDef.Named("Click")
            };
            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    action = delegate { DrillScanningRemainingTicks -= 30000; },
                    defaultLabel = "Debug: Progress Scann",
                    defaultDesc = "Debug: Progress Scann",
                    activateSound = SoundDef.Named("Click")
                };
            }
        }

        // Token: 0x06000025 RID: 37 RVA: 0x000027B7 File Offset: 0x000009B7
        public override void PostDeSpawn(Map map)
        {
            map.GetComponent<LaserDrillMapComp>().Deregister(this);
            SetRequiredDrillScanningToDefault();
            base.PostDeSpawn(map);
        }

        // Token: 0x06000026 RID: 38 RVA: 0x000027D4 File Offset: 0x000009D4
        private bool IsValidForActivation()
        {
            if (IsScanComplete() & HasSufficientShipResources())
            {
                return true;
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Laser Activation Failure:");
            if (!IsScanComplete())
            {
                if (IsScanning())
                {
                    stringBuilder.AppendLine(" * Scanning incomplete - Time Remaining: " +
                                             DrillScanningRemainingTicks.ToStringTicksToPeriod());
                }
                else
                {
                    stringBuilder.AppendLine(" * Scanning paused - Time Remaining after resuming: " +
                                             DrillScanningRemainingTicks.ToStringTicksToPeriod());
                }
            }

            if (!HasSufficientShipResources())
            {
                stringBuilder.AppendLine(" * " + m_RequiresShipResourcesComp.StatusString);
            }

            Find.LetterStack.ReceiveLetter("Scann in progress", stringBuilder.ToString(), LetterDefOf.NeutralEvent,
                new LookTargets(parent));
            Messages.Message(stringBuilder.ToString(), MessageTypeDefOf.NegativeEvent);
            return false;
        }

        // Token: 0x06000027 RID: 39 RVA: 0x000028BF File Offset: 0x00000ABF
        private void SetRequiredDrillScanningToDefault()
        {
            DrillScanningRemainingTicks = Mod_Laser_Drill.Settings.RequiredScanningTimeDays * 60000;
        }

        // Token: 0x06000028 RID: 40 RVA: 0x000028D8 File Offset: 0x00000AD8
        public Thing FindClosestGeyserToPoint(IntVec3 location)
        {
            var list = parent.Map.listerThings.ThingsOfDef(ThingDefOf.SteamGeyser);
            Thing result = null;
            var num = double.MaxValue;
            foreach (var thing in list)
            {
                if (!thing.Spawned || !location.InHorDistOf(thing.Position, 5f))
                {
                    continue;
                }

                var num2 = Math.Sqrt(Math.Pow(location.x - thing.Position.x, 2.0) +
                                     Math.Pow(location.y - thing.Position.y, 2.0));
                if (!(num2 < num))
                {
                    continue;
                }

                num = num2;
                result = thing;
            }

            return result;
        }

        // Token: 0x06000029 RID: 41 RVA: 0x000029B8 File Offset: 0x00000BB8
        public void TriggerLaserToFill()
        {
            var targetingParameters = new TargetingParameters
            {
                canTargetLocations = true
            };
            Find.Targeter.BeginTargeting(targetingParameters, delegate(LocalTargetInfo target)
            {
                var location = new IntVec3(target.Cell.x, target.Cell.y, target.Cell.z);
                if (!IsValidForActivation())
                {
                    return;
                }

                var thing = FindClosestGeyserToPoint(location);
                if (thing != null)
                {
                    ShowLaserVisually(thing.Position);
                    thing.DeSpawn();
                    m_RequiresShipResourcesComp.UseResources();
                    Messages.Message("SteamGeyser Removed.", MessageTypeDefOf.TaskCompletion);
                    parent.Destroy();
                    return;
                }

                Messages.Message("SteamGeyser not found to Remove.", MessageTypeDefOf.NegativeEvent);
            }, delegate(LocalTargetInfo target) { GenDraw.DrawRadiusRing(target.Cell, 5f); }, null);
        }

        // Token: 0x0600002A RID: 42 RVA: 0x00002A0C File Offset: 0x00000C0C
        public void TriggerLaser()
        {
            var targetingParameters = new TargetingParameters
            {
                canTargetLocations = true
            };
            Find.Targeter.BeginTargeting(targetingParameters, delegate(LocalTargetInfo target)
            {
                var intVec = new IntVec3(target.Cell.x, target.Cell.y, target.Cell.z);
                if (!IsValidForActivation())
                {
                    return;
                }

                ShowLaserVisually(intVec);
                GenSpawn.Spawn(ThingDef.Named("SteamGeyser"), intVec, parent.Map);
                m_RequiresShipResourcesComp.UseResources();
                Messages.Message("SteamGeyser Created.", MessageTypeDefOf.TaskCompletion);
                parent.Destroy();
            }, delegate(LocalTargetInfo target)
            {
                GenDraw.DrawRadiusRing(target.Cell, 0.1f);
                GenDraw.DrawRadiusRing(new IntVec3(target.Cell.x + 1, target.Cell.y, target.Cell.z), 0.1f);
                GenDraw.DrawRadiusRing(new IntVec3(target.Cell.x, target.Cell.y, target.Cell.z + 1), 0.1f);
                GenDraw.DrawRadiusRing(new IntVec3(target.Cell.x + 1, target.Cell.y, target.Cell.z + 1), 0.1f);
            }, null);
        }

        // Token: 0x0600002B RID: 43 RVA: 0x00002A60 File Offset: 0x00000C60
        private void ShowLaserVisually(IntVec3 position)
        {
            var unused =
                (LaserDrillVisual) GenSpawn.Spawn(ThingDef.Named("LaserDrillVisual"), position, parent.Map);
        }

        // Token: 0x0600002C RID: 44 RVA: 0x00002A84 File Offset: 0x00000C84
        private bool IsScanComplete()
        {
            return DrillScanningRemainingTicks <= 0;
        }

        // Token: 0x0600002D RID: 45 RVA: 0x00002A92 File Offset: 0x00000C92
        private bool HasPowerToScan()
        {
            return m_PowerComp == null || m_PowerComp.PowerOn;
        }

        // Token: 0x0600002E RID: 46 RVA: 0x00002AA9 File Offset: 0x00000CA9
        public bool IsScanning()
        {
            return !IsScanComplete() & HasPowerToScan();
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00002ABC File Offset: 0x00000CBC
        public void StopScanning()
        {
            if (!(!m_FlickComp.WantsFlick() & m_FlickComp.SwitchIsOn))
            {
                return;
            }

            ((Command_Toggle) m_FlickComp.CompGetGizmosExtra().ToList().First()).toggleAction();
            m_FlickComp.SwitchIsOn = false;
            Messages.Message("Drill Shutdown, Multiple Drills Scanning at once will cause interference.", parent,
                MessageTypeDefOf.RejectInput);
        }
    }
}