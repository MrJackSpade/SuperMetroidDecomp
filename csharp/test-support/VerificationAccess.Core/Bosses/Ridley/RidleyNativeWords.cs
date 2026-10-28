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

    /// <summary>The bank-$A6 routine word naming each <see cref="RidleyAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<RidleyAiFunction> AiFunctions = new(new Dictionary<RidleyAiFunction, ushort>
    {
        [RidleyAiFunction.ClearVelocity] = 0xa354,
        [RidleyAiFunction.WaitForDoorTransition] = 0xa35b,
        [RidleyAiFunction.InitialDelay] = 0xa377,
        [RidleyAiFunction.FadeInEyes] = 0xa389,
        [RidleyAiFunction.FadeInBody] = 0xa3df,
        [RidleyAiFunction.WaitBeforeRoar] = 0xa455,
        [RidleyAiFunction.WaitBeforeLiftoff] = 0xa478,
        [RidleyAiFunction.CeresLiftoffAccelerating] = 0xa6af,
        [RidleyAiFunction.CeresLiftoffDecelerating] = 0xa6c8,
        [RidleyAiFunction.CeresHovering] = 0xa6e8,
        [RidleyAiFunction.CeresFireballMoveToPosition] = 0xa782,
        [RidleyAiFunction.CeresFireballShooting] = 0xa7f9,
        [RidleyAiFunction.CeresLungeSetup] = 0xa83c,
        [RidleyAiFunction.CeresLungeMain] = 0xa84e,
        [RidleyAiFunction.CeresSwoopSetup] = 0xa88d,
        [RidleyAiFunction.CeresSwoopMoveToPosition] = 0xa8a4,
        [RidleyAiFunction.CeresSwoopDescendingAimingDown] = 0xa8d4,
        [RidleyAiFunction.CeresSwoopDescendingAimingLeft] = 0xa8f8,
        [RidleyAiFunction.CeresSwoopAscendingAimingUp] = 0xa923,
        [RidleyAiFunction.CeresSwoopAscendingAimingUpFaster] = 0xa947,
        [RidleyAiFunction.CeresRealRetreatRising] = 0xa971,
        [RidleyAiFunction.CeresRetreatDelay] = 0xa9a0,
        [RidleyAiFunction.CeresPublishEscapeHandoff] = 0xaa11,
        [RidleyAiFunction.CeresInactive] = 0xaa1b,
        [RidleyAiFunction.CeresActivateSelfDestruct] = 0xc04e,
        [RidleyAiFunction.CeresSelfDestructPaletteOnly] = 0xaa50,
        [RidleyAiFunction.NorfairEnterArena] = 0xb2f3,
        [RidleyAiFunction.NorfairSelectAttack] = 0xb321,
        [RidleyAiFunction.NorfairHoverSetup] = 0xb3ec,
        [RidleyAiFunction.NorfairHover] = 0xb3f8,
        [RidleyAiFunction.NorfairSwoopSetup] = 0xb441,
        [RidleyAiFunction.NorfairSwoopMoveToStart] = 0xb455,
        [RidleyAiFunction.NorfairSwoopAimDown] = 0xb493,
        [RidleyAiFunction.NorfairSwoopAimSideways] = 0xb4d1,
        [RidleyAiFunction.NorfairSwoopAimUp] = 0xb516,
        [RidleyAiFunction.NorfairSwoopClimb] = 0xb554,
        [RidleyAiFunction.NorfairSwoopRecover] = 0xb594,
        [RidleyAiFunction.NorfairPogoSetup] = 0xb5c4,
        [RidleyAiFunction.NorfairPogoDescending] = 0xb5e5,
        [RidleyAiFunction.NorfairPogoAscending] = 0xb613,
        [RidleyAiFunction.NorfairFireballSetup] = 0xb68b,
        [RidleyAiFunction.NorfairFireballMoveToSide] = 0xb6a7,
        [RidleyAiFunction.NorfairFireballMoveToHeight] = 0xb6dd,
        [RidleyAiFunction.NorfairFireballAttack] = 0xb70e,
        [RidleyAiFunction.NorfairFireballRecover] = 0xb7b9,
        [RidleyAiFunction.NorfairGrabApproach] = 0xbab7,
        [RidleyAiFunction.NorfairCarrySetup] = 0xbb8f,
        [RidleyAiFunction.NorfairCarryMoveToAnchor] = 0xbbc4,
        [RidleyAiFunction.NorfairCarryRise] = 0xbbf1,
        [RidleyAiFunction.NorfairCarryRelease] = 0xbc2e,
        [RidleyAiFunction.NorfairReturnToArena] = 0xbd4e,
        [RidleyAiFunction.CeresFakeRetreatMoveToPosition] = 0xbd9a,
        [RidleyAiFunction.CeresFakeRetreatRising] = 0xbdbc,
        [RidleyAiFunction.CeresWaitBeforeRetrievingBaby] = 0xbdf2,
        [RidleyAiFunction.CeresRetrieveBaby] = 0xbe03,
        [RidleyAiFunction.NorfairReleaseSamus] = 0xc538,
        [RidleyAiFunction.NorfairDeathStart] = 0xc53e,
        [RidleyAiFunction.NorfairDeathExplosions] = 0xc551,
        [RidleyAiFunction.NorfairDeathFall] = 0xc588,
        [RidleyAiFunction.NorfairDeathImpact] = 0xc5a8,
        [RidleyAiFunction.NorfairDeathWait] = 0xc5c8,
        [RidleyAiFunction.NorfairDeathFinish] = 0xc5da,
        [RidleyAiFunction.NorfairDeathComplete] = 0xc600,
    });
}
