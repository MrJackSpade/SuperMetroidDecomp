
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Zero crawler OAM identities shown by its four surface-orientation loops.
/// Orientation changes, velocity, timers and collision stay in the crawler code.
/// </summary>
internal static class ZeroVisualDefinitions
{
    /// <summary>Zero instruction and OAM bank $A3.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Sixteen selected Spritemap_Zero records at $A3:995B..9B6A.</summary>
    internal const int FrameCount = 16;
}
