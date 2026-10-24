using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Frontend;

/// <summary>
/// Compares a frame's painted sprite draws with the entries their screen placed. The paint resolves each entry's colours
/// (#1408), so the draws are compared as placed, before colouring.
/// </summary>
internal static class PaintedSpriteEntries
{
    /// <summary>Whether <paramref name="painted"/> holds exactly <paramref name="placed"/>, in order.</summary>
    /// <param name="painted">The draws a frame's paint holds.</param>
    /// <param name="placed">The entries the screen placed this frame.</param>
    internal static bool AreOwn(ImmutableArray<SpriteDraw> painted, ImmutableArray<SpriteDraw> placed) =>
        painted.Select(AsPlaced).SequenceEqual(placed.Select(AsPlaced));

    /// <summary>A draw without the colours the paint resolved for it.</summary>
    private static SpriteDraw AsPlaced(SpriteDraw draw) => draw is ImageSpriteDraw image ? image with { Colours = null } : draw;
}
