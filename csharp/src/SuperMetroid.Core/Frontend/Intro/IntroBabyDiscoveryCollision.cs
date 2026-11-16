using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Frontend.Intro.IntroBabyDiscoveryCollisionDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Builds the intro baby-discovery room's collision (<see cref="IntroBabyDiscoveryCollisionDefinitions"/>) from its two
/// horizontal strips.
/// </summary>
internal static class IntroBabyDiscoveryCollision
{
    /// <summary>How each block of the room collides, row by row, built freshly from its two horizontal collision strips.</summary>
    internal static RoomCollisionType[] CreateCollision()
    {
        var foreground = new RoomCollisionType[Columns * RoomRows];
        foreground.AsSpan().Fill(EmptyBlock);
        foreground.AsSpan(ZeroRows * Columns, Columns).Fill(PenultimateRowBlock);
        foreground.AsSpan((SourceRows - 1) * Columns, Columns).Fill(FinalRowBlock);
        return foreground;
    }
}
