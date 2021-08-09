using System.Linq;
using RimWorld;
using Verse;

namespace Jaxxa.EnhancedDevelopment.LaserDrill.Things
{
    // Token: 0x02000004 RID: 4
    internal class LaserDrillVisual : ThingWithComps
    {
        // Token: 0x04000004 RID: 4
        private static readonly SimpleCurve DistanceChanceFactor;

        // Token: 0x04000005 RID: 5
        private static readonly FloatRange AngleRange;

        // Token: 0x04000006 RID: 6
        private float Angle;

        // Token: 0x04000007 RID: 7
        public int Duration = 600;

        // Token: 0x04000008 RID: 8
        private int StartTick;

        // Token: 0x0600000F RID: 15 RVA: 0x00002370 File Offset: 0x00000570
        // Note: this type is marked as 'beforefieldinit'.
        static LaserDrillVisual()
        {
            var simpleCurve = new SimpleCurve {new CurvePoint(0f, 1f), new CurvePoint(10f, 0f)};
            DistanceChanceFactor = simpleCurve;
            AngleRange = new FloatRange(-12f, 12f);
        }

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x0600000C RID: 12 RVA: 0x0000233B File Offset: 0x0000053B
        protected int TicksLeft => Duration - TicksPassed;

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x0600000D RID: 13 RVA: 0x0000234A File Offset: 0x0000054A
        protected int TicksPassed => Find.TickManager.TicksGame - StartTick;

        // Token: 0x06000007 RID: 7 RVA: 0x000021B8 File Offset: 0x000003B8
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

        // Token: 0x06000008 RID: 8 RVA: 0x0000224B File Offset: 0x0000044B
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

        // Token: 0x06000009 RID: 9 RVA: 0x00002288 File Offset: 0x00000488
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Duration, "Duration");
            Scribe_Values.Look(ref Angle, "Angle");
            Scribe_Values.Look(ref StartTick, "StartTick");
        }

        // Token: 0x0600000A RID: 10 RVA: 0x000022D5 File Offset: 0x000004D5
        public override void Draw()
        {
            Comps_PostDraw();
        }

        // Token: 0x0600000B RID: 11 RVA: 0x000022E0 File Offset: 0x000004E0
        private void StartRandomFire()
        {
            FireUtility.TryStartFireIn((from x in GenRadial.RadialCellsAround(Position, 25f, true)
                    where x.InBounds(Map)
                    select x).RandomElementByWeight(x => DistanceChanceFactor.Evaluate(x.DistanceTo(Position))), Map,
                Rand.Range(0.1f, 0.925f));
        }
    }
}