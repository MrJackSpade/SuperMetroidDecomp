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

    /// <summary>Where <paramref name="painted"/> first departs from <paramref name="placed"/>, for a failure message.</summary>
    /// <param name="painted">The draws a frame's paint holds.</param>
    /// <param name="placed">The entries the screen placed this frame.</param>
    internal static string FirstDifference(ImmutableArray<SpriteDraw> painted, ImmutableArray<SpriteDraw> placed)
    {
        for (int index = 0; index < Math.Min(painted.Length, placed.Length); index++)
        {
            if (!painted[index].Equals(placed[index]))
                return $"entry {index}: painted {painted[index]}, placed {placed[index]}";
        }
        int shorter = Math.Min(painted.Length, placed.Length);
        string unmatched = painted.Length > shorter ? $"painted extra {painted[shorter]}"
            : placed.Length > shorter ? $"placed extra {placed[shorter]}" : "none";
        return $"painted {painted.Length} entries, placed {placed.Length}; first unmatched: {unmatched}";
    }
}
