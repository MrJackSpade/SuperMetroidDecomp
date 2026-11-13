using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Verification's native view of Draygon's identities (#1413): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class DraygonNativeWords
{
    /// <summary>The bank-$A5 routine word naming each <see cref="DraygonEyeAction"/>.</summary>
    internal static readonly NativeWordIdentities<DraygonEyeAction> EyeActions = new(new Dictionary<DraygonEyeAction, ushort>
    {
        [DraygonEyeAction.Inert] = 0x804b,
        [DraygonEyeAction.FacingLeft] = 0xc48d,
        [DraygonEyeAction.FacingRight] = 0xc513,
    });

    /// <summary>Pre-instruction <c>$84:DB64</c>, accepting missiles and Super Missiles.</summary>
    public const ushort MissileHitFrameBehavior = 0xdb64;

    /// <summary>
    /// The bytes of one swoop path entry: the cartridge's record keeps the swoop entry and the path's length as byte
    /// distances into a table of four-byte entries (#1473).
    /// </summary>
    public const int SwoopEntryBytes = 4;

    /// <summary>The work-memory offset of the cartridge's swoop path table: each entry's height is its first word.</summary>
    private const int SwoopPathTable = 0x9002;

    /// <summary>
    /// The swoop path a native snapshot holds: <paramref name="lengthBytes"/> (the record's path length, a byte distance)
    /// covers the climb's entries, and the entry after them holds the height the swoop started at.
    /// </summary>
    /// <param name="word">Reads the snapshot's work-memory word at an offset.</param>
    /// <param name="lengthBytes">The recorded path length in bytes.</param>
    internal static DraygonSwoopPath SwoopPathAt(Func<int, ushort> word, int lengthBytes)
    {
        int count = lengthBytes / SwoopEntryBytes;
        System.Collections.Immutable.ImmutableArray<int> heights = [.. Enumerable.Range(0, count).Select(entry => (int)word(SwoopPathTable + entry * SwoopEntryBytes))];
        return new DraygonSwoopPath(heights, word(SwoopPathTable + count * SwoopEntryBytes));
    }

    /// <summary>What the record word the cartridge shares between three of Draygon's values holds.</summary>
    internal enum SharedValue
    {
        /// <summary>The current intro stage's frame count.</summary>
        IntroFrames,
        /// <summary>The dying body's burial wait.</summary>
        BurialWait,
        /// <summary>The swoop path entry.</summary>
        SwoopEntry,
    }

    /// <summary>
    /// What the record word the cartridge shares between the intro frame count, the burial wait and the swoop entry
    /// holds during <paramref name="phase"/>: the intro count before the first swoop, the burial wait while dying,
    /// and the swoop entry otherwise.
    /// </summary>
    internal static SharedValue MeaningOfSharedValue(DraygonPhase phase) =>
        phase is DraygonPhase.None or DraygonPhase.IntroInitialDelay or DraygonPhase.IntroDance
            ? SharedValue.IntroFrames
            : phase is DraygonPhase.Dying or DraygonPhase.DyingSink or DraygonPhase.DyingFinish
                ? SharedValue.BurialWait
                : SharedValue.SwoopEntry;
}
