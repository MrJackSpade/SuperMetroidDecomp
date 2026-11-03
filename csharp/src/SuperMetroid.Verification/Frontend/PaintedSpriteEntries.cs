using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Frontend;

/// <summary>
/// Compares a frame's painted sprite draws with the entries their screen placed. A frontend screen's paint keeps each
/// entry as placed, shading included (#1512), so the draws must be the same draws.
/// </summary>
internal static class PaintedSpriteEntries
{
    /// <summary>Whether <paramref name="painted"/> holds exactly <paramref name="placed"/>, in order.</summary>
    /// <param name="painted">The draws a frame's paint holds.</param>
    /// <param name="placed">The entries the screen placed this frame.</param>
    internal static bool AreOwn(ImmutableArray<SpriteDraw> painted, ImmutableArray<SpriteDraw> placed) =>
        painted.SequenceEqual(placed);
}
