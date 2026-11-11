using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The scrolling credits roll (#1555, #1601): its staff lines' pictures, laid out by <see cref="CreditsRollLines"/>, and
/// how far the roll has scrolled up the screen.
/// </summary>
/// <remarks>
/// The roll scrolls half a pixel an update and finishes on the update it has scrolled
/// <see cref="CreditsRollLines.FinishingScroll"/> pixels. The screen shows every line at its place on the roll, the roll's
/// top starting <see cref="StartDistance"/> pixels below the view's top, moved up by the distance scrolled; nothing wraps,
/// and no line is held or replaced.
/// </remarks>
internal sealed class CreditsObjectState
{
    /// <summary>Pixels below the view's top the roll's top starts at, more than the screen's height, so each line comes up from below the view.</summary>
    internal const int StartDistance = 256;
    /// <summary>The distance the roll has scrolled up, in pixels and fractions of a pixel.</summary>
    private SignedSixteenSixteen scrolled;

    /// <summary>Whether the roll still scrolls.</summary>
    public bool Enabled { get; private set; } = true;
    /// <summary>Set when the roll has scrolled past its end.</summary>
    public bool Finished { get; private set; }
    /// <summary>The whole pixels the roll has scrolled up.</summary>
    public int ScrolledPixels => scrolled.WholePixels;

    /// <summary>Runs one credits step: scrolls, and finishes once the roll has scrolled past its end.</summary>
    public void Step()
    {
        if (!Enabled)
            return;

        scrolled += EndingMotionDefinitions.CreditsScrollVelocity;
        if (ScrolledPixels < CreditsRollLines.FinishingScroll)
            return;
        Finished = true;
        Enabled = false;
    }

    /// <summary>
    /// The roll: each line's picture from <paramref name="lines"/> at its place, <see cref="StartDistance"/> pixels below
    /// the roll's top, in <paramref name="shading"/>, the ending's colours (#1512); the screen shows it moved up by
    /// <see cref="ScrolledPixels"/>.
    /// </summary>
    internal PicturedPlane Plane(SpriteSheet lines, PictureShading shading)
    {
        Ensure.NotNull(lines);
        Ensure.NotNull(shading);
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>(CreditsRollLines.Lines.Length);
        foreach (CreditsRollLine line in CreditsRollLines.Lines)
            placed.Add(new PlacedPlanePicture(lines.Frame(line.Picture), Offset16<PlanePixels>.Of(0, StartDistance + line.Top), shading));
        return new PicturedPlane(placed.MoveToImmutable());
    }
}
