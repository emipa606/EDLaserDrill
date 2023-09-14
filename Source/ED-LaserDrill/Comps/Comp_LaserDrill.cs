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

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Comps;

[StaticConstructorOnStartup]
internal class Comp_LaserDrill : ThingComp
{
    private static readonly Texture2D UI_LASER_ACTIVATE = ContentFinder<Texture2D>.Get("UI/Power/SteamGeyser");

    private static readonly Texture2D UI_LASER_ACTIVATEFILL =
        ContentFinder<Texture2D>.Get("UI/Power/RemoveSteamGeyser");

    private int DrillScanningRemainingTicks;

    private CompFlickable m_FlickComp;

    private CompPowerTrader m_PowerComp;

    private IRequiresShipResources m_RequiresShipResourcesComp;

    private CompProperties_LaserDrill Properties;

    private bool HasSufficientShipResources()
    {
        return m_RequiresShipResourcesComp.Satisfied;
    }

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

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref DrillScanningRemainingTicks, "DrillScanningRemainingTicks");
    }

    public override void CompTickRare()
    {
        if (IsScanning())
        {
            DrillScanningRemainingTicks -= 250;
        }

        base.CompTickRare();
    }

    public override string CompInspectStringExtra()
    {
        var stringBuilder = new StringBuilder();
        if (IsScanComplete())
        {
            stringBuilder.AppendLine("EDL.scancomplete".Translate());
        }
        else if (HasPowerToScan())
        {
            stringBuilder.AppendLine(
                "EDL.scaninprogress".Translate(DrillScanningRemainingTicks.ToStringTicksToPeriod()));
        }
        else
        {
            stringBuilder.AppendLine("EDL.scanpaused".Translate());
        }


        stringBuilder.Append(m_RequiresShipResourcesComp.StatusString);
        return stringBuilder.ToString();
    }

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
            defaultLabel = "EDL.activatelaser".Translate(),
            defaultDesc = "EDL.activatelaserTT".Translate(),
            activateSound = SoundDef.Named("Click")
        };

        if (ModLister.GetActiveModWithIdentifier("VanillaExpanded.HelixienGas") != null)
        {
            yield return new Command_Action
            {
                action = TriggerHelixienLaser,
                icon = ContentFinder<Texture2D>.Get("Things/Building/Natural/GasGeyser"),
                defaultLabel = "EDL.activatehelixienlaser".Translate(),
                defaultDesc = "EDL.activatehelixienlaserTT".Translate(),
                activateSound = SoundDef.Named("Click")
            };
        }

        yield return new Command_Action
        {
            action = TriggerLaserToFill,
            icon = UI_LASER_ACTIVATEFILL,
            defaultLabel = "EDL.activatelaserfill".Translate(),
            defaultDesc = "EDL.activatelaserfill".Translate(),
            activateSound = SoundDef.Named("Click")
        };
        if (DebugSettings.godMode)
        {
            yield return new Command_Action
            {
                action = delegate { DrillScanningRemainingTicks -= 30000; },
                defaultLabel = "EDL.debugscan".Translate(),
                defaultDesc = "EDL.debugscan".Translate(),
                activateSound = SoundDef.Named("Click")
            };
        }
    }

    public override void PostDeSpawn(Map map)
    {
        map.GetComponent<LaserDrillMapComp>().Deregister(this);
        SetRequiredDrillScanningToDefault();
        base.PostDeSpawn(map);
    }

    private bool IsValidForActivation()
    {
        if (IsScanComplete() & HasSufficientShipResources())
        {
            return true;
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("EDL.laseractivationfailure".Translate());
        if (!IsScanComplete())
        {
            stringBuilder.AppendLine(
                IsScanning()
                    ? "EDL.scanningincomplete".Translate(DrillScanningRemainingTicks.ToStringTicksToPeriod())
                    : "EDL.scanningpaused".Translate(DrillScanningRemainingTicks.ToStringTicksToPeriod()));
        }

        if (!HasSufficientShipResources())
        {
            stringBuilder.AppendLine($" * {m_RequiresShipResourcesComp.StatusString}");
        }

        Find.LetterStack.ReceiveLetter("EDL.scaninginprogress".Translate(), stringBuilder.ToString(),
            LetterDefOf.NeutralEvent,
            new LookTargets(parent));
        Messages.Message(stringBuilder.ToString(), MessageTypeDefOf.NegativeEvent);
        return false;
    }

    private void SetRequiredDrillScanningToDefault()
    {
        DrillScanningRemainingTicks = Mod_Laser_Drill.Settings.RequiredScanningTimeDays * 60000;
    }

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
                Messages.Message("EDL.steamgeyserremoved".Translate(), MessageTypeDefOf.TaskCompletion);
                parent.Destroy();
                return;
            }

            Messages.Message("EDL.steamgeysernotfound".Translate(), MessageTypeDefOf.NegativeEvent);
        }, delegate(LocalTargetInfo target) { GenDraw.DrawRadiusRing(target.Cell, 5f); }, null);
    }

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
            GenSpawn.Spawn(ThingDefOf.SteamGeyser, intVec, parent.Map);
            m_RequiresShipResourcesComp.UseResources();
            Messages.Message("EDL.steamgeysercreated".Translate(), MessageTypeDefOf.TaskCompletion);
            parent.Destroy();
        }, delegate(LocalTargetInfo target)
        {
            GenDraw.DrawRadiusRing(target.Cell, 0.1f);
            GenDraw.DrawRadiusRing(new IntVec3(target.Cell.x + 1, target.Cell.y, target.Cell.z), 0.1f);
            GenDraw.DrawRadiusRing(new IntVec3(target.Cell.x, target.Cell.y, target.Cell.z + 1), 0.1f);
            GenDraw.DrawRadiusRing(new IntVec3(target.Cell.x + 1, target.Cell.y, target.Cell.z + 1), 0.1f);
        }, null);
    }

    public void TriggerHelixienLaser()
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
            GenSpawn.Spawn(ThingDef.Named("VHGE_GasGeyser"), intVec, parent.Map);
            m_RequiresShipResourcesComp.UseResources();
            Messages.Message("EDL.helixiengeysercreated".Translate(), MessageTypeDefOf.TaskCompletion);
            parent.Destroy();
        }, delegate(LocalTargetInfo target)
        {
            GenDraw.DrawRadiusRing(target.Cell, 0.1f);
            GenDraw.DrawRadiusRing(new IntVec3(target.Cell.x + 1, target.Cell.y, target.Cell.z), 0.1f);
            GenDraw.DrawRadiusRing(new IntVec3(target.Cell.x, target.Cell.y, target.Cell.z + 1), 0.1f);
            GenDraw.DrawRadiusRing(new IntVec3(target.Cell.x + 1, target.Cell.y, target.Cell.z + 1), 0.1f);
        }, null);
    }

    private void ShowLaserVisually(IntVec3 position)
    {
        var unused =
            (LaserDrillVisual)GenSpawn.Spawn(ThingDef.Named("LaserDrillVisual"), position, parent.Map);
    }

    private bool IsScanComplete()
    {
        return DrillScanningRemainingTicks <= 0;
    }

    private bool HasPowerToScan()
    {
        return m_PowerComp == null || m_PowerComp.PowerOn;
    }

    public bool IsScanning()
    {
        return !IsScanComplete() & HasPowerToScan();
    }

    public void StopScanning()
    {
        if (!(!m_FlickComp.WantsFlick() & m_FlickComp.SwitchIsOn))
        {
            return;
        }

        ((Command_Toggle)m_FlickComp.CompGetGizmosExtra().ToList().First()).toggleAction();
        m_FlickComp.SwitchIsOn = false;
        Messages.Message("EDL.drillshutdown".Translate(), parent,
            MessageTypeDefOf.RejectInput);
    }
}