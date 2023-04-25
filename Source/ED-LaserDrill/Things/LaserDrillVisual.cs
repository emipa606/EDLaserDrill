using System.Linq;
using RimWorld;
using Verse;

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Things;

internal class LaserDrillVisual : ThingWithComps
{
    private static readonly SimpleCurve DistanceChanceFactor;

    private static readonly FloatRange AngleRange;

    private float Angle;

    public int Duration = 600;

    private int StartTick;

    // Note: this type is marked as 'beforefieldinit'.
    static LaserDrillVisual()
    {
        var simpleCurve = new SimpleCurve { new CurvePoint(0f, 1f), new CurvePoint(10f, 0f) };
        DistanceChanceFactor = simpleCurve;
        AngleRange = new FloatRange(-12f, 12f);
    }

    protected int TicksLeft => Duration - TicksPassed;

    protected int TicksPassed => Find.TickManager.TicksGame - StartTick;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        Angle = AngleRange.RandomInRange;
        StartTick = Find.TickManager.TicksGame;
        GetComp<CompAffectsSky>().StartFadeInHoldFadeOut(30, Duration - 30 - 15, 15);
        GetComp<CompOrbitalBeam>().StartAnimation(Duration, 10, Angle);
        MoteMaker.MakeBombardmentMote(Position, Map, 1f);
        MoteMaker.MakePowerBeamMote(Position, Map);
    }

    public override void Tick()
    {
        base.Tick();
        if (TicksPassed >= Duration)
        {
            Destroy();
        }

        if (!Destroyed && Find.TickManager.TicksGame % 50 == 0)
        {
            StartRandomFire();
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref Duration, "Duration");
        Scribe_Values.Look(ref Angle, "Angle");
        Scribe_Values.Look(ref StartTick, "StartTick");
    }

    public override void Draw()
    {
        Comps_PostDraw();
    }

    private void StartRandomFire()
    {
        FireUtility.TryStartFireIn((from x in GenRadial.RadialCellsAround(Position, 25f, true)
                where x.InBounds(Map)
                select x).RandomElementByWeight(x => DistanceChanceFactor.Evaluate(x.DistanceTo(Position))), Map,
            Rand.Range(0.1f, 0.925f));
    }
}