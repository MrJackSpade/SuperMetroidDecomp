using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The presentation boundary of the eye-door sweat drop (#1296): the installed artwork key of every typed pose, the
/// bank-$86 address of the spritemap operand that drew it.
/// </summary>
internal static class EyeDoorSweatVisualDefinitions
{
    /// <summary>The falling pose's list at $86:B615.</summary>
    private const ushort FallingProgram = 0xb615;
    /// <summary>The splash list at $86:B61D, whose first frame follows its clear-pre-instruction command.</summary>
    private const ushort HitFloorProgram = 0xb61d;
    /// <summary>Bytes of the splash list's opening command.</summary>
    private const int CommandBytes = 2;
    /// <summary>A frame's spritemap operand follows its two-byte duration.</summary>
    private const int OperandOffset = 2;
    /// <summary>Each timed frame is a duration and an operand.</summary>
    private const int FrameBytes = 4;

    /// <summary>Every pose in installed-artwork order.</summary>
    internal static IReadOnlyList<EyeDoorSweatFrame> ProjectileFrames { get; } = Enum.GetValues<EyeDoorSweatFrame>();

    /// <summary>The installed presentation key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(EyeDoorSweatFrame frame) => unchecked((ushort)(frame switch
    {
        EyeDoorSweatFrame.Drop => FallingProgram + OperandOffset,
        EyeDoorSweatFrame.SplashBegin or EyeDoorSweatFrame.SplashSpread or EyeDoorSweatFrame.SplashEnd =>
            HitFloorProgram + CommandBytes + FrameBytes * (frame - EyeDoorSweatFrame.SplashBegin) + OperandOffset,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined eye-door sweat frame."),
    }));
}