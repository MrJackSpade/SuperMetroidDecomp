using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Decodes the cartridge's arm-cannon cover frame numbers: each direction's list holds a null entry at zero for the closed
/// cover and the slightly, half and fully open pictures at one through three.
/// </summary>
internal static class SamusArmCannonCoverFrameCodec
{
    /// <summary>Every cover picture, in the order an opening shows them.</summary>
    internal static IReadOnlyList<SamusArmCannonCoverFrame> InOpeningOrder { get; } =
        [SamusArmCannonCoverFrame.SlightlyOpen, SamusArmCannonCoverFrame.HalfOpen, SamusArmCannonCoverFrame.FullyOpen];

    /// <summary>The cartridge's frame number of the closed cover in a direction's cover list, which uploads no picture.</summary>
    internal const int ClosedFrame = 0;

    /// <summary>The cartridge's frame number of <paramref name="frame"/> in a direction's cover list, 1 through 3.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not a cover picture.</exception>
    internal static int CartridgeFrame(SamusArmCannonCoverFrame frame) => frame switch
    {
        SamusArmCannonCoverFrame.SlightlyOpen => 1,
        SamusArmCannonCoverFrame.HalfOpen => 2,
        SamusArmCannonCoverFrame.FullyOpen => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
}
