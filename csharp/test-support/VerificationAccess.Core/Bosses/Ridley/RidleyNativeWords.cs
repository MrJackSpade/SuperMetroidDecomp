using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Verification's native view of the Ridley identities (#1413): the cartridge words naming each member. Core's values are
/// arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class RidleyNativeWords
{
    /// <summary>The facing index ($7E:7820) each <see cref="RidleyFacing"/> is stored as; the arena-target tables are indexed by it (#1452).</summary>
    internal static readonly NativeWordIdentities<RidleyFacing> Facings = new(new Dictionary<RidleyFacing, ushort>
    {
        [RidleyFacing.Left] = 0,
        [RidleyFacing.Turning] = 1,
        [RidleyFacing.Right] = 2,
    });

    /// <summary>
    /// The health-stage word ($7E:7824) each <see cref="NorfairRidleyHealthBand"/> is stored as (#1934): the number of
    /// health thresholds passed, which indexes the cartridge's divisor tables and, two further on, its pogo tables.
    /// </summary>
    internal static readonly NativeWordIdentities<NorfairRidleyHealthBand> HealthBands = new(new Dictionary<NorfairRidleyHealthBand, ushort>
    {
        [NorfairRidleyHealthBandDefinitions.Unhurt] = 0,
        [NorfairRidleyHealthBandDefinitions.Wounded] = 1,
        [NorfairRidleyHealthBandDefinitions.BadlyWounded] = 2,
        [NorfairRidleyHealthBandDefinitions.NearlyDead] = 3,
    });

    /// <summary>The bands in the order of their native health-stage words.</summary>
    internal static NorfairRidleyHealthBand[] HealthBandsInStageOrder =>
    [
        NorfairRidleyHealthBandDefinitions.Unhurt, NorfairRidleyHealthBandDefinitions.Wounded,
        NorfairRidleyHealthBandDefinitions.BadlyWounded, NorfairRidleyHealthBandDefinitions.NearlyDead,
    ];

    /// <summary>
    /// The encounter-mode word ($7E:7802) holding each <see cref="RidleyFightMode"/> (#1452). The cartridge tests it with
    /// BEQ, BMI and CMP #2; dying is its only negative value.
    /// </summary>
    internal static readonly NativeWordIdentities<RidleyFightMode> FightModes = new(new Dictionary<RidleyFightMode, ushort>
    {
        [RidleyFightMode.NotFighting] = 0x0000,
        [RidleyFightMode.Fighting] = 0x0001,
        [RidleyFightMode.PowerBombDodge] = 0x0002,
        [RidleyFightMode.Dying] = 0xffff,
    });

    /// <summary>
    /// The tail-function index ($7E:2000) naming each <see cref="RidleyTailAction"/>. The cartridge's table has two more
    /// entries, seven and eight, that nothing selects.
    /// </summary>
    internal static readonly NativeWordIdentities<RidleyTailAction> TailActions = new(new Dictionary<RidleyTailAction, ushort>
    {
        [RidleyTailAction.None] = 0,
        [RidleyTailAction.Neutral] = 1,
        [RidleyTailAction.PogoSetup] = 2,
        [RidleyTailAction.PointDown] = 3,
        [RidleyTailAction.Pogo] = 4,
        [RidleyTailAction.StabSetup] = 5,
        [RidleyTailAction.Stab] = 6,
    });


    /// <summary>
    /// The bank-$A6 routine word naming each <see cref="RidleyRevealPhase"/> (#2227): both encounters store these in their
    /// AI-function word before lift-off.
    /// </summary>
    internal static readonly NativeWordIdentities<RidleyRevealPhase> RevealPhases = new(new Dictionary<RidleyRevealPhase, ushort>
    {
        [RidleyRevealPhase.TakingOff] = 0xa354,
        [RidleyRevealPhase.WaitForDoorTransition] = 0xa35b,
        [RidleyRevealPhase.InitialDelay] = 0xa377,
        [RidleyRevealPhase.FadeInEyes] = 0xa389,
        [RidleyRevealPhase.FadeInBody] = 0xa3df,
        [RidleyRevealPhase.WaitBeforeRoar] = 0xa455,
        [RidleyRevealPhase.WaitBeforeLiftoff] = 0xa478,
    });

    /// <summary>The bank-$A6 routine word naming each <see cref="CeresRidleyPhase"/> (#2227).</summary>
    internal static readonly NativeWordIdentities<CeresRidleyPhase> CeresPhases = new(new Dictionary<CeresRidleyPhase, ushort>
    {
        [CeresRidleyPhase.LiftoffAccelerating] = 0xa6af,
        [CeresRidleyPhase.LiftoffDecelerating] = 0xa6c8,
        [CeresRidleyPhase.Hovering] = 0xa6e8,
        [CeresRidleyPhase.FireballMoveToPosition] = 0xa782,
        [CeresRidleyPhase.FireballShooting] = 0xa7f9,
        [CeresRidleyPhase.LungeSetup] = 0xa83c,
        [CeresRidleyPhase.LungeMain] = 0xa84e,
        [CeresRidleyPhase.SwoopSetup] = 0xa88d,
        [CeresRidleyPhase.SwoopMoveToPosition] = 0xa8a4,
        [CeresRidleyPhase.SwoopDescendingAimingDown] = 0xa8d4,
        [CeresRidleyPhase.SwoopDescendingAimingLeft] = 0xa8f8,
        [CeresRidleyPhase.SwoopAscendingAimingUp] = 0xa923,
        [CeresRidleyPhase.SwoopAscendingAimingUpFaster] = 0xa947,
        [CeresRidleyPhase.RealRetreatRising] = 0xa971,
        [CeresRidleyPhase.RetreatDelay] = 0xa9a0,
        [CeresRidleyPhase.PublishEscapeHandoff] = 0xaa11,
        [CeresRidleyPhase.Inactive] = 0xaa1b,
        [CeresRidleyPhase.ActivateSelfDestruct] = 0xc04e,
        [CeresRidleyPhase.SelfDestructAlarmOnly] = 0xaa50,
        [CeresRidleyPhase.FakeRetreatMoveToPosition] = 0xbd9a,
        [CeresRidleyPhase.FakeRetreatRising] = 0xbdbc,
        [CeresRidleyPhase.WaitBeforeRetrievingBaby] = 0xbdf2,
        [CeresRidleyPhase.RetrieveBaby] = 0xbe03,
    });

    /// <summary>
    /// The bank-$A6 routine word naming each <see cref="NorfairRidleyPhase"/> (#2227). The cartridge's pogo fly-to-position
    /// entry ($B68B) and death-roar entry ($C53E) are only ever jumped to, never stored, so no phase names them (#1939).
    /// </summary>
    internal static readonly NativeWordIdentities<NorfairRidleyPhase> NorfairPhases = new(new Dictionary<NorfairRidleyPhase, ushort>
    {
        [NorfairRidleyPhase.EnterArena] = 0xb2f3,
        [NorfairRidleyPhase.SelectAttack] = 0xb321,
        [NorfairRidleyPhase.HoverSetup] = 0xb3ec,
        [NorfairRidleyPhase.Hover] = 0xb3f8,
        [NorfairRidleyPhase.SwoopSetup] = 0xb441,
        [NorfairRidleyPhase.SwoopMoveToStart] = 0xb455,
        [NorfairRidleyPhase.SwoopAimDown] = 0xb493,
        [NorfairRidleyPhase.SwoopAimSideways] = 0xb4d1,
        [NorfairRidleyPhase.SwoopAimUp] = 0xb516,
        [NorfairRidleyPhase.SwoopClimb] = 0xb554,
        [NorfairRidleyPhase.SwoopRecover] = 0xb594,
        [NorfairRidleyPhase.WallSweepSetup] = 0xb5c4,
        [NorfairRidleyPhase.SweepToWallBehind] = 0xb5e5,
        [NorfairRidleyPhase.SweepToWallAhead] = 0xb613,
        [NorfairRidleyPhase.PogoMoveToSide] = 0xb6a7,
        [NorfairRidleyPhase.PogoMoveToHeight] = 0xb6dd,
        [NorfairRidleyPhase.PogoFall] = 0xb70e,
        [NorfairRidleyPhase.PogoRise] = 0xb7b9,
        [NorfairRidleyPhase.GrabApproach] = 0xbab7,
        [NorfairRidleyPhase.CarrySetup] = 0xbb8f,
        [NorfairRidleyPhase.CarryMoveToAnchor] = 0xbbc4,
        [NorfairRidleyPhase.CarryRise] = 0xbbf1,
        [NorfairRidleyPhase.CarryRelease] = 0xbc2e,
        [NorfairRidleyPhase.PowerBombDodge] = 0xbd4e,
        [NorfairRidleyPhase.MoveToDeathSpot] = 0xc538,
        [NorfairRidleyPhase.DeathRoar] = 0xc551,
        [NorfairRidleyPhase.DeathExplosions] = 0xc588,
        [NorfairRidleyPhase.DeathBreakup] = 0xc5a8,
        [NorfairRidleyPhase.DeathWait] = 0xc5c8,
        [NorfairRidleyPhase.DeathFinish] = 0xc5da,
        [NorfairRidleyPhase.DeathComplete] = 0xc600,
    });

    /// <summary>
    /// The AI-function word Lower Norfair Ridley's <paramref name="state"/> stands for: his reveal's phase until he lifts
    /// off, then his own.
    /// </summary>
    internal static ushort NorfairPhaseWord(RidleyEnemyState state) =>
        state.LiftedOff ? NorfairPhases.Word(state.NorfairPhase) : RevealPhases.Word(state.RevealPhase);
}
