using SuperMetroid.Core.Samus.Movement;
using static SuperMetroid.Core.Frontend.Intro.IntroBabyDiscoveryCollisionDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Builds the intro baby-discovery room's collision (<see cref="IntroBabyDiscoveryCollisionDefinitions"/>): its row of
/// slopes on its floor.
/// </summary>
internal static partial class IntroBabyDiscoveryCollision
{
    /// <summary>How each block of the room collides, row by row, built freshly: air, with the slope row on the floor row.</summary>
    internal static PlainTerrainBlock[] CreateCollision()
    {
        var foreground = new PlainTerrainBlock[Columns * RoomRows];
        foreground.AsSpan().Fill(EmptyBlock);
        foreground.AsSpan(SlopeRow * Columns, Columns).Fill(SlopeBlock);
        foreground.AsSpan(FloorRow * Columns, Columns).Fill(FloorBlock);
        return foreground;
    }
}
