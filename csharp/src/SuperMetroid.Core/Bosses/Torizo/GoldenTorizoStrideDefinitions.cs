using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Golden Torizo's planted-foot stride: where the planted foot's left edge sits, relative to the body's origin, in each
/// of the ten walking phases. The walk derives each phase's horizontal advance from these positions; the import pipeline
/// places the walking frames' foot strips at the same positions.
/// </summary>
internal static class GoldenTorizoStrideDefinitions
{
    /// <summary>The incoming support foot begins 37 pixels left of the origin.</summary>
    private const int ForwardPlantX = -37;
    /// <summary>The first leg's early planted-foot origin, 32 pixels left of the origin.</summary>
    private const int FirstEarlySupportX = -32;
    /// <summary>The second leg's early planted-foot origin, 30 pixels left of the origin.</summary>
    private const int SecondEarlySupportX = -30;
    /// <summary>Both legs pass the same middle support origin, 13 pixels left of the origin.</summary>
    private const int MiddleSupportX = -13;
    /// <summary>The first leg's final single-foot support origin, 3 pixels right of the origin.</summary>
    private const int FirstPushOffX = 3;
    /// <summary>The second leg's final single-foot support origin, 5 pixels right of the origin.</summary>
    private const int SecondPushOffX = 5;

    /// <summary>Double-plant poses put the outgoing foot 10 pixels right of the origin.</summary>
    internal const int DepartingFootX = 10;

    /// <summary>The walking phases in each half of the stride, one per leg; each half opens with a double plant.</summary>
    internal const int PhasesPerHalf = 5;

    /// <summary>
    /// The planted foot's left edge by walking phase. Each five-phase half plants the incoming foot for two phases, then
    /// passes its early, middle and push-off origins; only the early and push-off origins differ between the halves.
    /// </summary>
    internal static ImmutableArray<int> SupportFootXs { get; } =
    [
        ForwardPlantX, ForwardPlantX, FirstEarlySupportX, MiddleSupportX, FirstPushOffX,
        ForwardPlantX, ForwardPlantX, SecondEarlySupportX, MiddleSupportX, SecondPushOffX,
    ];
}
