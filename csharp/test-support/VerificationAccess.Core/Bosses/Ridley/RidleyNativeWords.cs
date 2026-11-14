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

    /// <summary>The bank-$A6 routine word naming each <see cref="RidleyPhase"/>.</summary>
    internal static readonly NativeWordIdentities<RidleyPhase> Phases = new(new Dictionary<RidleyPhase, ushort>
    {
        [RidleyPhase.ClearVelocity] = 0xa354,
        [RidleyPhase.WaitForDoorTransition] = 0xa35b,
        [RidleyPhase.InitialDelay] = 0xa377,
        [RidleyPhase.FadeInEyes] = 0xa389,
        [RidleyPhase.FadeInBody] = 0xa3df,
        [RidleyPhase.WaitBeforeRoar] = 0xa455,
        [RidleyPhase.WaitBeforeLiftoff] = 0xa478,
        [RidleyPhase.CeresLiftoffAccelerating] = 0xa6af,
        [RidleyPhase.CeresLiftoffDecelerating] = 0xa6c8,
        [RidleyPhase.CeresHovering] = 0xa6e8,
        [RidleyPhase.CeresFireballMoveToPosition] = 0xa782,
        [RidleyPhase.CeresFireballShooting] = 0xa7f9,
        [RidleyPhase.CeresLungeSetup] = 0xa83c,
        [RidleyPhase.CeresLungeMain] = 0xa84e,
        [RidleyPhase.CeresSwoopSetup] = 0xa88d,
        [RidleyPhase.CeresSwoopMoveToPosition] = 0xa8a4,
        [RidleyPhase.CeresSwoopDescendingAimingDown] = 0xa8d4,
        [RidleyPhase.CeresSwoopDescendingAimingLeft] = 0xa8f8,
        [RidleyPhase.CeresSwoopAscendingAimingUp] = 0xa923,
        [RidleyPhase.CeresSwoopAscendingAimingUpFaster] = 0xa947,
        [RidleyPhase.CeresRealRetreatRising] = 0xa971,
        [RidleyPhase.CeresRetreatDelay] = 0xa9a0,
        [RidleyPhase.CeresPublishEscapeHandoff] = 0xaa11,
        [RidleyPhase.CeresInactive] = 0xaa1b,
        [RidleyPhase.CeresActivateSelfDestruct] = 0xc04e,
        [RidleyPhase.CeresSelfDestructAlarmOnly] = 0xaa50,
        [RidleyPhase.NorfairEnterArena] = 0xb2f3,
        [RidleyPhase.NorfairSelectAttack] = 0xb321,
        [RidleyPhase.NorfairHoverSetup] = 0xb3ec,
        [RidleyPhase.NorfairHover] = 0xb3f8,
        [RidleyPhase.NorfairSwoopSetup] = 0xb441,
        [RidleyPhase.NorfairSwoopMoveToStart] = 0xb455,
        [RidleyPhase.NorfairSwoopAimDown] = 0xb493,
        [RidleyPhase.NorfairSwoopAimSideways] = 0xb4d1,
        [RidleyPhase.NorfairSwoopAimUp] = 0xb516,
        [RidleyPhase.NorfairSwoopClimb] = 0xb554,
        [RidleyPhase.NorfairSwoopRecover] = 0xb594,
        [RidleyPhase.NorfairPogoSetup] = 0xb5c4,
        [RidleyPhase.NorfairPogoDescending] = 0xb5e5,
        [RidleyPhase.NorfairPogoAscending] = 0xb613,
        [RidleyPhase.NorfairFireballSetup] = 0xb68b,
        [RidleyPhase.NorfairFireballMoveToSide] = 0xb6a7,
        [RidleyPhase.NorfairFireballMoveToHeight] = 0xb6dd,
        [RidleyPhase.NorfairFireballAttack] = 0xb70e,
        [RidleyPhase.NorfairFireballRecover] = 0xb7b9,
        [RidleyPhase.NorfairGrabApproach] = 0xbab7,
        [RidleyPhase.NorfairCarrySetup] = 0xbb8f,
        [RidleyPhase.NorfairCarryMoveToAnchor] = 0xbbc4,
        [RidleyPhase.NorfairCarryRise] = 0xbbf1,
        [RidleyPhase.NorfairCarryRelease] = 0xbc2e,
        [RidleyPhase.NorfairReturnToArena] = 0xbd4e,
        [RidleyPhase.CeresFakeRetreatMoveToPosition] = 0xbd9a,
        [RidleyPhase.CeresFakeRetreatRising] = 0xbdbc,
        [RidleyPhase.CeresWaitBeforeRetrievingBaby] = 0xbdf2,
        [RidleyPhase.CeresRetrieveBaby] = 0xbe03,
        [RidleyPhase.NorfairReleaseSamus] = 0xc538,
        [RidleyPhase.NorfairDeathStart] = 0xc53e,
        [RidleyPhase.NorfairDeathExplosions] = 0xc551,
        [RidleyPhase.NorfairDeathFall] = 0xc588,
        [RidleyPhase.NorfairDeathImpact] = 0xc5a8,
        [RidleyPhase.NorfairDeathWait] = 0xc5c8,
        [RidleyPhase.NorfairDeathFinish] = 0xc5da,
        [RidleyPhase.NorfairDeathComplete] = 0xc600,
    });
}
