using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Frontend.Intro.IntroBabyDiscoveryCollisionDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Builds the intro baby-discovery room's collision (<see cref="IntroBabyDiscoveryCollisionDefinitions"/>) from its two
/// horizontal strips.
/// </summary>
internal static class IntroBabyDiscoveryCollision
{
    /// <summary>Construct mutable room terrain directly from its two horizontal collision strips.</summary>
    internal static RoomTerrainBlock[] CreateForeground()
    {
        var foreground = new RoomTerrainBlock[Columns * RoomRows];
        foreground.AsSpan().Fill(EmptyBlock);
        foreground.AsSpan(ZeroRows * Columns, Columns).Fill(PenultimateRowBlock);
        foreground.AsSpan((SourceRows - 1) * Columns, Columns).Fill(FinalRowBlock);
        return foreground;
    }
}
