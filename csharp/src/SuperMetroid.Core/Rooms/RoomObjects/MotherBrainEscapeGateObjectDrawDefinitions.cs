using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The block shape of the three frames of the Mother Brain escape gate (#1608). What each frame shows is its installed
/// picture (#1557).
/// </summary>
internal static partial class MotherBrainEscapeGateObjectDrawDefinitions
{
    /// <summary>The gate: four solid blocks down its column from the origin, in every frame, the visually open one included.</summary>
    private static readonly RoomBlockShape Gate = RoomBlockShapes.Column(4, RoomCollisionType.SolidBlock);

    /// <summary>The block shape each of the three gate frames draws.</summary>
    internal static readonly Dictionary<MotherBrainEscapeGateDraw, RoomBlockShape> Shapes = new()
    {
        [MotherBrainEscapeGateDraw.Open] = Gate,
        [MotherBrainEscapeGateDraw.HalfClosed] = Gate,
        [MotherBrainEscapeGateDraw.Closed] = Gate,
    };
}
