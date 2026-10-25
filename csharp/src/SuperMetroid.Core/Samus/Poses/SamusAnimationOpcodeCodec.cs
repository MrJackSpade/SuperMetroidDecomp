namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The codec between <see cref="SamusAnimationCommand"/> and the control opcode bytes that end each run of frame
/// delays in the bank-$91 animation delay streams. Core keeps the streams as bytes because the animation interpreter
/// reads them by offset, including the over-reads past a pose's own program; this is the one place a command becomes
/// its opcode.
/// </summary>
[SnesEncodingOwner]
internal static class SamusAnimationOpcodeCodec
{
    /// <summary>The opcode byte that encodes a control command.</summary>
    internal static byte Opcode(SamusAnimationCommand command) => command switch
    {
        SamusAnimationCommand.Hold => 0xf0,
        SamusAnimationCommand.LoopUnlessLowEnergy => 0xf6,
        SamusAnimationCommand.InstallDrainedFall => 0xf7,
        SamusAnimationCommand.TurnTransition => 0xf8,
        SamusAnimationCommand.ItemAirborneTransition => 0xf9,
        SamusAnimationCommand.WallJumpSelect => 0xfb,
        SamusAnimationCommand.ItemTransition => 0xfc,
        SamusAnimationCommand.Transition => 0xfd,
        SamusAnimationCommand.RepeatFrom => 0xfe,
        SamusAnimationCommand.Loop => 0xff,
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusAnimationCommand)} {command}."),
    };
}
