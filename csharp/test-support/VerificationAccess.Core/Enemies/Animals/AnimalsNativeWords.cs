using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// Verification's native view of the Animals identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class AnimalsNativeWords
{
    /// <summary>The routine word naming each <see cref="DachoraAction"/>.</summary>
    internal static readonly NativeWordIdentities<DachoraAction> DachoraActions = new(new Dictionary<DachoraAction, ushort>
    {
        [DachoraAction.WaitingForSamus] = 0xf570,
        [DachoraAction.BlinkingBeforeRun] = 0xf5bc,
        [DachoraAction.RunningLeft] = 0xf5ed,
        [DachoraAction.RunningRight] = 0xf65e,
        [DachoraAction.ChargingShinespark] = 0xf78f,
        [DachoraAction.Shinesparking] = 0xf806,
        [DachoraAction.Falling] = 0xf935,
    });

    /// <summary>The routine word a Dachora speed-boost echo's record names, the echo's flicker (#2147).</summary>
    internal const ushort DachoraEchoRoutine = 0xf98c;

    /// <summary>
    /// The routine word naming each <see cref="EscapeEtecoonAction"/>: an Etecoon thanking Samus runs the routine that
    /// returns at once (#1803), its animation moving it.
    /// </summary>
    internal static readonly NativeWordIdentities<EscapeEtecoonAction> EscapeEtecoonActions = new(new Dictionary<EscapeEtecoonAction, ushort>
    {
        [EscapeEtecoonAction.Escaping] = 0xe65c,
        [EscapeEtecoonAction.AwaitingEscape] = 0xe670,
        [EscapeEtecoonAction.Walking] = 0xe680,
        [EscapeEtecoonAction.ExpressingGratitude] = 0x807b,
    });

    /// <summary>The escape Etecoon action a recorded routine word names.</summary>
    internal static EscapeEtecoonAction EscapeEtecoonActionAt(ushort word) =>
        EscapeEtecoonActions.Decode(word, "recorded EscapeEtecoonAction");

    /// <summary>
    /// The routine word naming each <see cref="EtecoonAction"/>. One routine runs both the dormant wait and the flex, which
    /// the record's countdown tells apart (#2476): it names <see cref="EtecoonAction.Dormant"/>.
    /// </summary>
    internal static readonly NativeWordIdentities<EtecoonAction> EtecoonActions = new(new Dictionary<EtecoonAction, ushort>
    {
        [EtecoonAction.Dormant] = 0xe9af,
        [EtecoonAction.PreJumpCountdown] = 0xea00,
        [EtecoonAction.InitialJump] = 0xea37,
        [EtecoonAction.FaceSamusPause] = 0xeab5,
        [EtecoonAction.RunLeftToWall] = 0xeb02,
        [EtecoonAction.RunRightToWall] = 0xeb2c,
        [EtecoonAction.Airborne] = 0xeb50,
        [EtecoonAction.WallPause] = 0xebcd,
        [EtecoonAction.RouteAfterLanding] = 0xec1b,
        [EtecoonAction.MoveLeftToTeachingStart] = 0xec97,
        [EtecoonAction.MoveRightToTeachingStart] = 0xecbb,
        [EtecoonAction.RunRightToLongJump] = 0xecdf,
        [EtecoonAction.LongJump] = 0xed09,
        [EtecoonAction.RunRightAfterLongJump] = 0xed2a,
        [EtecoonAction.LongJumpLanding] = 0xed54,
        [EtecoonAction.TeachingJump] = 0xed75,
        [EtecoonAction.IdleBetweenTeachingJumps] = 0xedc7,
        [EtecoonAction.WaitForSamusBeforeLongRun] = 0xee3e,
        [EtecoonAction.RunRightToReturnJump] = 0xee9a,
        [EtecoonAction.ReturnJump] = 0xeeb8,
    });
}
