using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>
/// Checks the map animations' own limits (#1627): the highlight cycle wraps at its frame count and holds each frame for
/// its count of updates, with no byte limit on either, and a map arrow's cycle has at least one phase.
/// </summary>
internal static class MapAnimationLimitsVerification
{
    /// <summary>A 300-frame cycle whose frames hold 300 updates steps through every frame and wraps once, at its end.</summary>
    internal static void VerifyMapAnimationLimits()
    {
        const int frames = 300, hold = 300;
        var cycle = new MapHighlightCycle([.. Enumerable.Repeat(hold, frames)]);
        AssertEqual(frames, cycle.FrameCount, "the cycle keeps more than 255 frames");
        var animation = new MapHighlightAnimation();
        animation.Bind(cycle);
        int wraps = 0;
        // The cycle starts one update from its second frame; every frame after that holds its full count, through the
        // wrap to the first frame and up to the update before the second frame shows again.
        for (int update = 1; update < 1 + frames * hold; update++)
        {
            bool wrapped = animation.Step();
            if (wrapped)
                wraps++;
            int expectedFrame = (update - 1) / hold + 1;
            AssertEqual(expectedFrame == frames ? 0 : expectedFrame, animation.Frame, $"update {update}: the frame the holds have reached");
            AssertEqual(update == 1 + (frames - 1) * hold, wrapped, $"update {update}: the cycle wraps only past its last frame");
        }
        AssertEqual(1, wraps, "one pass through the cycle wraps once");
        AssertThrows<InvalidDataException>(() => new MapHighlightCycle([]), "a highlight cycle has at least one frame");
        AssertThrows<InvalidDataException>(() => new MapHighlightCycle([1, -1]), "a highlight hold is not below zero");
        AssertThrows<InvalidDataException>(() => new MapArrowVisual(new(0, 0), []), "a map arrow cycles through at least one phase");
    }
}
