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
        [DachoraAction.Echo] = 0xf98c,
    });

    /// <summary>The routine word naming each <see cref="EscapeEtecoonFrameBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<EscapeEtecoonFrameBehavior> EscapeEtecoonFrameBehaviors = new(new Dictionary<EscapeEtecoonFrameBehavior, ushort>
    {
        [EscapeEtecoonFrameBehavior.Cleared] = 0x807b,
        [EscapeEtecoonFrameBehavior.EscapeRight] = 0xe65c,
        [EscapeEtecoonFrameBehavior.WaitForEscapeEvent] = 0xe670,
        [EscapeEtecoonFrameBehavior.WalkAndFall] = 0xe680,
    });

    /// <summary>The routine word naming each <see cref="EtecoonAction"/>.</summary>
    internal static readonly NativeWordIdentities<EtecoonAction> EtecoonActions = new(new Dictionary<EtecoonAction, ushort>
    {
        [EtecoonAction.WaitingForSamus] = 0xe9af,
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
