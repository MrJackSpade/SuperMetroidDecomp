namespace SuperMetroid.Core.Fx;

/// <summary>
/// The dimension of the cartridge's per-area block-animation selection lists, which <see cref="RoomFxRecordCodec"/>
/// resolves a record's block-animation bits through (#1526).
/// </summary>
/// <remarks>
/// Each area owns eight object selections, chosen by the eight bits in an FX record's block-animation bitset. The
/// decoded room effect names the block animations themselves; the selection is the import's knowledge.
/// </remarks>
internal static class AreaBlockAnimationDefinitions
{
    /// <summary>Eight object-header words are addressable by one FX bitset.</summary>
    public const int ObjectsPerArea = 8;
}
