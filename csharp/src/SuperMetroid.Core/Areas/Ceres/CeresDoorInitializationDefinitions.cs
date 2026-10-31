using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Compiled behavior selectors used by <c>InitAI_CeresDoor</c> at <c>$A6:F6C5</c>.
/// </summary>
internal static class CeresDoorInitializationDefinitions
{

    /// <summary>
    /// <c>InitAI_CeresDoor.functionPointers</c> at <c>$A6:F72B-$A6:F738</c> paired
    /// with <c>InstListPointers_CeresDoor</c> at <c>$A6:F52C-$A6:F539</c>.
    /// For full-word population variant i = 0..6, the instruction-list
    /// pointer at $F52C + 2*i is respectively $F56C, $F5BE, $F610,
    /// $F53A, $F61A, $F62A, or $F634. These authored entries select the
    /// normal right/left, pre-explosion, Ridley-room, invisible-wall, and
    /// two mode-7 wall programs; variant seven is outside the table.
    /// The separate main-function pointer at $F72B + 2*i is $F76B for
    /// variants 0, 1, and 4; $F7BD for 2; $F770 for 3; and $F7A5 for 5
    /// and 6. Those aliases classify authored behavior, rather than a
    /// numeric progression of code addresses.
    /// </summary>
    internal static readonly IReadOnlyDictionary<CeresDoorVariant, CeresDoorInitializationDefinition> Initializations =
        new Dictionary<CeresDoorVariant, CeresDoorInitializationDefinition>
        {
            [CeresDoorVariant.NormalFacingRight] = new(
                CeresDoorFunction.HandleEarthquakeDuringEscape,
                CeresDoorProgram.NormalFacingRight),
            [CeresDoorVariant.NormalFacingLeft] = new(
                CeresDoorFunction.HandleEarthquakeDuringEscape,
                CeresDoorProgram.NormalFacingLeft),
            [CeresDoorVariant.RotatingElevatorPreExplosionOverlay] = new(
                CeresDoorFunction.RotatingElevatorRoomDefault,
                CeresDoorProgram.RotatingElevatorPreExplosionOverlay),
            [CeresDoorVariant.RidleyRoomFacingRight] = new(
                CeresDoorFunction.HandleEarthquakeDuringEscapeInRidleysRoom,
                CeresDoorProgram.RidleyRoomFacingRight),
            [CeresDoorVariant.RotatingElevatorInvisibleWall] = new(
                CeresDoorFunction.HandleEarthquakeDuringEscape,
                CeresDoorProgram.RotatingElevatorInvisibleWall),
            [CeresDoorVariant.RidleyEscapeFloorLeftWall] = new(
                CeresDoorFunction.RidleyEscapeFloorWall,
                CeresDoorProgram.RidleyEscapeFloorLeftWall),
            [CeresDoorVariant.RidleyEscapeFloorRightWall] = new(
                CeresDoorFunction.RidleyEscapeFloorWall,
                CeresDoorProgram.RidleyEscapeFloorRightWall),
        };
}
