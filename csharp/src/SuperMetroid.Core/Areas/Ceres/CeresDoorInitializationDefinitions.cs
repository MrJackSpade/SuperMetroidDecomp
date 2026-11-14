
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// What each kind of Ceres door and wall does once it is placed in its room: the behaviour it runs and the animation it
/// starts in.
/// </summary>
internal static class CeresDoorInitializationDefinitions
{

    /// <summary>
    /// Each kind of Ceres door and wall, with the behaviour it runs and the animation it starts in. The ordinary doors
    /// facing right and left and the rotating elevator room's invisible wall shake the screen while Ceres
    /// self-destructs; the door of Ridley's room shakes it in Ridley's room; the overlay the rotating elevator room
    /// shows before the explosion keeps that room's default behaviour; and the left and right walls beside Ridley's
    /// rotating escape floor run as that floor's walls. Several kinds share a behaviour because the game authored them
    /// alike, not because the kinds follow one another.
    /// </summary>
    internal static readonly IReadOnlyDictionary<CeresDoorVariant, CeresDoorInitializationDefinition> Initializations =
        new Dictionary<CeresDoorVariant, CeresDoorInitializationDefinition>
        {
            [CeresDoorVariant.NormalFacingRight] = new(
                CeresDoorAction.HandleEarthquakeDuringEscape,
                CeresDoorSegment.NormalFacingRight),
            [CeresDoorVariant.NormalFacingLeft] = new(
                CeresDoorAction.HandleEarthquakeDuringEscape,
                CeresDoorSegment.NormalFacingLeft),
            [CeresDoorVariant.RotatingElevatorPreExplosionOverlay] = new(
                CeresDoorAction.RotatingElevatorRoomDefault,
                CeresDoorSegment.RotatingElevatorPreExplosionOverlay),
            [CeresDoorVariant.RidleyRoomFacingRight] = new(
                CeresDoorAction.HandleEarthquakeDuringEscapeInRidleysRoom,
                CeresDoorSegment.RidleyRoomFacingRight),
            [CeresDoorVariant.RotatingElevatorInvisibleWall] = new(
                CeresDoorAction.HandleEarthquakeDuringEscape,
                CeresDoorSegment.RotatingElevatorInvisibleWall),
            [CeresDoorVariant.RidleyEscapeFloorLeftWall] = new(
                CeresDoorAction.RidleyEscapeFloorWall,
                CeresDoorSegment.RidleyEscapeFloorLeftWall),
            [CeresDoorVariant.RidleyEscapeFloorRightWall] = new(
                CeresDoorAction.RidleyEscapeFloorWall,
                CeresDoorSegment.RidleyEscapeFloorRightWall),
        };
}
