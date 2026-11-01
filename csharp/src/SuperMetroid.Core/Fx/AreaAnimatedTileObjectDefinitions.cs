namespace SuperMetroid.Core.Fx;

/// <summary>
/// Immutable area-to-animated-tile-object selection metadata consumed by
/// <c>LoadFxHeader</c>.
/// </summary>
/// <remarks>
/// Each area owns eight object selections, chosen by the eight bits in
/// an FX record's animated-tile bitset. These selections choose object behavior; the
/// selected objects' character-frame operands and graphics remain presentation data.
/// </remarks>
internal static class AreaAnimatedTileObjectDefinitions
{

    /// <summary>Eight object-header words are addressable by one FX bitset.</summary>
    public const int ObjectsPerArea = 8;
}
