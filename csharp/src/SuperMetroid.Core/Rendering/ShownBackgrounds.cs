using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The background colours a frame shows, by the owner that holds them (#1464): gameplay's background sets, whose image
/// groups name the set they draw in, or a frontend screen's, whose image groups show the screen's set of the same
/// identity.
/// </summary>
internal abstract class ShownBackgrounds
{
    /// <summary>The colour shown where nothing is drawn.</summary>
    internal abstract RgbColor Backdrop { get; }

    /// <summary>The colours background colour group <paramref name="group"/> of <paramref name="sheet"/> shows.</summary>
    internal abstract ReadOnlySpan<RgbColor> Group(SpriteImage sheet, int group);

    /// <summary>These colours with the backdrop shown in <paramref name="backdrop"/>.</summary>
    internal abstract ShownBackgrounds WithBackdrop(RgbColor backdrop);
}
