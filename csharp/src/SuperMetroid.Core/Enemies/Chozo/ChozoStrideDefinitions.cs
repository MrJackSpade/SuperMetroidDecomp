using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Where the Chozo statue plants its support foot in each of the four stride phases, as a horizontal distance from the
/// statue's origin to the foot's left edge. The carry step derives its velocity from adjacent phases; the import
/// pipeline places the stride poses' foot glyphs at the same distances.
/// </summary>
internal static class ChozoStrideDefinitions
{
    /// <summary>$AA:E959: the weight-shift phase plants the foot 31 pixels left of the origin.</summary>
    private const int WeightShiftFootX = -31;

    /// <summary>$AA:E9C4: the early support phase plants the foot 28 pixels left of the origin.</summary>
    private const int EarlySupportFootX = -28;

    /// <summary>$AA:EA34: the late support phase plants the foot 14 pixels left of the origin.</summary>
    private const int LateSupportFootX = -14;

    /// <summary>$AA:EAA4: the push-off phase plants the foot 7 pixels left of the origin.</summary>
    private const int PushOffFootX = -7;

    /// <summary>The support foot's left edge, relative to the statue's origin, for each of the four stride phases in order.</summary>
    internal static ImmutableArray<int> SupportFootXs { get; } = [WeightShiftFootX, EarlySupportFootX, LateSupportFootX, PushOffFootX];
}
