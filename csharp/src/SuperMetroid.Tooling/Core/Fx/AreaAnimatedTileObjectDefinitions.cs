namespace SuperMetroid.Core.Fx;

/// <summary>
/// The dimension of the cartridge's per-area animated-tile selection lists, which <see cref="RoomFxRecordCodec"/>
/// resolves a record's animated-tile bits through (#1526).
/// </summary>
/// <remarks>
/// Each area owns eight object selections, chosen by the eight bits in an FX record's animated-tile bitset. The
/// decoded record names the objects themselves; the selection is the import's knowledge.
/// </remarks>
internal static class AreaAnimatedTileObjectDefinitions
{
    /// <summary>Eight object-header words are addressable by one FX bitset.</summary>
    public const int ObjectsPerArea = 8;
}
