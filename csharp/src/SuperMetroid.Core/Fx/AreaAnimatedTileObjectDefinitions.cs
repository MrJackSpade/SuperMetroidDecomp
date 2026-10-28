namespace SuperMetroid.Core.Fx;

/// <summary>
/// Immutable bank-$83 area-to-animated-tile-object selection metadata consumed by
/// <c>LoadFxHeader</c>.
/// </summary>
/// <remarks>
/// Each area owns eight bank-$87 object-header pointers, selected by the eight bits in
/// an FX record's animated-tile bitset. These pointers choose object behavior; the
/// selected objects' character-frame operands and graphics remain presentation data.
/// </remarks>
internal static class AreaAnimatedTileObjectDefinitions
{

    /// <summary>Eight object-header words are addressable by one FX bitset.</summary>
    public const int ObjectsPerArea = 8;
}
