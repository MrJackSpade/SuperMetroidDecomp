
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Compiled behavior selectors used by the Ceres door initializer.
/// </summary>
internal static class CeresDoorInitializationDefinitions
{

    /// <summary>
    /// The per-variant behaviour and animation choices
    /// of the Ceres door initializer.
    /// Population variants 0..6 select, in order, the animations for the
    /// normal right/left, pre-explosion, Ridley-room, invisible-wall, and
    /// two mode-7 wall animations; variant seven is outside the table.
    /// The separate main behaviour is the escape earthquake for
    /// variants 0, 1, and 4; the rotating elevator room's default for 2; Ridley's-room earthquake for 3; and the rotating-floor wall for 5
    /// and 6. Those aliases classify authored behavior, rather than a
    /// numeric progression.
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
