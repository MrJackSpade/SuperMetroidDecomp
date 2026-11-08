using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Verification's native view of the Zoa identities (#1412): the cartridge routine words naming each member, and the
/// selector word's bit for each animation flag (#1549). Core's values are arbitrary; a native snapshot or ROM oracle
/// translates through these tables.
/// </summary>
internal static class ZoaNativeWords
{
    /// <summary>The routine word naming each <see cref="ZoaEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<ZoaEnemyFunction> ZoaEnemyFunctions = new(new Dictionary<ZoaEnemyFunction, ushort>
    {
        [ZoaEnemyFunction.WaitForSamus] = 0xb482,
        [ZoaEnemyFunction.Rising] = 0xb4a8,
        [ZoaEnemyFunction.Shooting] = 0xb4d6,
    });

    /// <summary>
    /// Each animation flag's bit in the selector word, which indexes the Zoa's four-entry animation table: the
    /// selector words are 0 through 3.
    /// </summary>
    private static readonly Dictionary<ZoaAnimationSelector, int> AnimationSelectorBits = new()
    {
        [ZoaAnimationSelector.Rising] = 1 << 0,
        [ZoaAnimationSelector.FacingRight] = 1 << 1,
    };

    /// <summary>The selector word, an index into the four-entry animation table, for <paramref name="selector"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="selector"/> holds a value no flag names.</exception>
    internal static int AnimationSelectorWord(ZoaAnimationSelector selector)
    {
        int word = 0;
        ZoaAnimationSelector encoded = ZoaAnimationSelector.None;
        foreach ((ZoaAnimationSelector flag, int bit) in AnimationSelectorBits)
        {
            if ((selector & flag) == ZoaAnimationSelector.None)
                continue;
            word |= bit;
            encoded |= flag;
        }
        return encoded == selector
            ? word
            : throw new ArgumentOutOfRangeException(nameof(selector), selector, "The value holds a bit no Zoa animation flag names.");
    }

    /// <summary>The animation flags a stored selector word holds.</summary>
    /// <exception cref="InvalidDataException">The word holds a bit no animation flag names.</exception>
    internal static ZoaAnimationSelector AnimationSelectorOf(int word)
    {
        ZoaAnimationSelector selector = ZoaAnimationSelector.None;
        foreach ((ZoaAnimationSelector flag, int bit) in AnimationSelectorBits)
            if ((word & bit) != 0)
                selector |= flag;
        return AnimationSelectorWord(selector) == word
            ? selector
            : throw new InvalidDataException($"Zoa animation selector word ${word:X4} holds a bit no animation flag names.");
    }
}
