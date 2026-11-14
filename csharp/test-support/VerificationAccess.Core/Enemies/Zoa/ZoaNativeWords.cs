using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Verification's native view of the Zoa identities (#1412): the cartridge routine words naming each member, and the
/// selector word naming each animation (#1549, #1624). Core's values are arbitrary; a native snapshot or ROM oracle
/// translates through these tables.
/// </summary>
internal static class ZoaNativeWords
{
    /// <summary>The routine word naming each <see cref="ZoaAction"/>.</summary>
    internal static readonly NativeWordIdentities<ZoaAction> ZoaActions = new(new Dictionary<ZoaAction, ushort>
    {
        [ZoaAction.WaitForSamus] = 0xb482,
        [ZoaAction.Rising] = 0xb4a8,
        [ZoaAction.Shooting] = 0xb4d6,
    });

    /// <summary>
    /// The selector word naming each animation, an index into the Zoa's four-entry animation table: its low bit selects
    /// rising and its next bit facing right.
    /// </summary>
    private static readonly NativeWordIdentities<ZoaSegment> AnimationSelectors = new(new Dictionary<ZoaSegment, ushort>
    {
        [ZoaSegment.FacingLeftShooting] = 0,
        [ZoaSegment.FacingLeftRising] = 1,
        [ZoaSegment.FacingRightShooting] = 2,
        [ZoaSegment.FacingRightRising] = 3,
    });

    /// <summary>The selector word, an index into the four-entry animation table, for <paramref name="selector"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="selector"/> names no animation.</exception>
    internal static int AnimationSelectorWord(ZoaSegment selector) => AnimationSelectors.Word(selector);

    /// <summary>The animation a stored selector word names.</summary>
    /// <exception cref="InvalidDataException">The word names no animation.</exception>
    internal static ZoaSegment AnimationSelectorOf(int word) => AnimationSelectors.Decode(word, "Zoa animation selector word");
}
