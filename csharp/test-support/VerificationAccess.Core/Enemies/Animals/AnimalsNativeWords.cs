using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// Verification's native view of the Animals identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class AnimalsNativeWords
{
    /// <summary>The routine word naming each <see cref="DachoraAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<DachoraAiFunction> DachoraAiFunctions = new(new Dictionary<DachoraAiFunction, ushort>
    {
        [DachoraAiFunction.WaitingForSamus] = 0xf570,
        [DachoraAiFunction.BlinkingBeforeRun] = 0xf5bc,
        [DachoraAiFunction.RunningLeft] = 0xf5ed,
        [DachoraAiFunction.RunningRight] = 0xf65e,
        [DachoraAiFunction.ChargingShinespark] = 0xf78f,
        [DachoraAiFunction.Shinesparking] = 0xf806,
        [DachoraAiFunction.Falling] = 0xf935,
        [DachoraAiFunction.Echo] = 0xf98c,
    });

    /// <summary>The routine word naming each <see cref="EscapeEtecoonPreInstruction"/>.</summary>
    internal static readonly NativeWordIdentities<EscapeEtecoonPreInstruction> EscapeEtecoonPreInstructions = new(new Dictionary<EscapeEtecoonPreInstruction, ushort>
    {
        [EscapeEtecoonPreInstruction.Cleared] = 0x807b,
        [EscapeEtecoonPreInstruction.EscapeRight] = 0xe65c,
        [EscapeEtecoonPreInstruction.WaitForEscapeEvent] = 0xe670,
        [EscapeEtecoonPreInstruction.WalkAndFall] = 0xe680,
    });

    /// <summary>The routine word naming each <see cref="EtecoonAiFunction"/>.</summary>
    internal static readonly NativeWordIdentities<EtecoonAiFunction> EtecoonAiFunctions = new(new Dictionary<EtecoonAiFunction, ushort>
    {
        [EtecoonAiFunction.WaitingForSamus] = 0xe9af,
        [EtecoonAiFunction.PreJumpCountdown] = 0xea00,
        [EtecoonAiFunction.InitialJump] = 0xea37,
        [EtecoonAiFunction.FaceSamusPause] = 0xeab5,
        [EtecoonAiFunction.RunLeftToWall] = 0xeb02,
        [EtecoonAiFunction.RunRightToWall] = 0xeb2c,
        [EtecoonAiFunction.Airborne] = 0xeb50,
        [EtecoonAiFunction.WallPause] = 0xebcd,
        [EtecoonAiFunction.RouteAfterLanding] = 0xec1b,
        [EtecoonAiFunction.MoveLeftToTeachingStart] = 0xec97,
        [EtecoonAiFunction.MoveRightToTeachingStart] = 0xecbb,
        [EtecoonAiFunction.RunRightToLongJump] = 0xecdf,
        [EtecoonAiFunction.LongJump] = 0xed09,
        [EtecoonAiFunction.RunRightAfterLongJump] = 0xed2a,
        [EtecoonAiFunction.LongJumpLanding] = 0xed54,
        [EtecoonAiFunction.TeachingJump] = 0xed75,
        [EtecoonAiFunction.IdleBetweenTeachingJumps] = 0xedc7,
        [EtecoonAiFunction.WaitForSamusBeforeLongRun] = 0xee3e,
        [EtecoonAiFunction.RunRightToReturnJump] = 0xee9a,
        [EtecoonAiFunction.ReturnJump] = 0xeeb8,
    });
}
