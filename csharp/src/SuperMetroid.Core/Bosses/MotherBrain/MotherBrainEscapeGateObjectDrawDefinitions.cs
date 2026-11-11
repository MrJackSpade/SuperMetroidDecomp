using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The block shape of the three frames of the Mother Brain escape gate (#1608). What each frame shows is its installed
/// picture (#1557).
/// </summary>
internal static class MotherBrainEscapeGateObjectDrawDefinitions
{
    /// <summary>The gate: four solid blocks down its column from the origin, in every frame, the visually open one included.</summary>
    private static readonly RoomBlockShape Gate = new(
    [
        new(new(0, 0), RoomCollisionType.SolidBlock),
        new(new(0, 1), RoomCollisionType.SolidBlock),
        new(new(0, 2), RoomCollisionType.SolidBlock),
        new(new(0, 3), RoomCollisionType.SolidBlock),
    ]);

    /// <summary>The block shape each of the three gate frames draws.</summary>
    internal static readonly Dictionary<MotherBrainEscapeGateDraw, RoomBlockShape> Shapes = new()
    {
        [MotherBrainEscapeGateDraw.Open] = Gate,
        [MotherBrainEscapeGateDraw.HalfClosed] = Gate,
        [MotherBrainEscapeGateDraw.Closed] = Gate,
    };
}
