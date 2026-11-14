using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Verification's native view of Draygon's identities (#1413): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class DraygonNativeWords
{
    /// <summary>The bank-$A5 routine word naming each <see cref="DraygonPhase"/> after the fight begins: the body's record word A.</summary>
    internal static readonly NativeWordIdentities<DraygonPhase> Phases = new(new Dictionary<DraygonPhase, ushort>
    {
        [DraygonPhase.IntroInitialDelay] = 0x871b,
        [DraygonPhase.IntroDance] = 0x878b,
        [DraygonPhase.SwoopRightSetup] = 0x87f4,
        [DraygonPhase.SwoopRightDescending] = 0x88b1,
        [DraygonPhase.SwoopRightApex] = 0x8922,
        [DraygonPhase.SwoopRightAscending] = 0x8951,
        [DraygonPhase.SwoopLeftSetup] = 0x89b3,
        [DraygonPhase.SwoopLeftDescending] = 0x8a00,
        [DraygonPhase.SwoopLeftApex] = 0x8a50,
        [DraygonPhase.SwoopLeftAscending] = 0x8a90,
        [DraygonPhase.GoopRightSetup] = 0x8b0a,
        [DraygonPhase.GoopRight] = 0x8b52,
        [DraygonPhase.GoopRightTail] = 0x8bae,
        [DraygonPhase.GoopRightRecovery] = 0x8c33,
        [DraygonPhase.GoopLeftSetup] = 0x8c8e,
        [DraygonPhase.GoopLeft] = 0x8cd4,
        [DraygonPhase.GoopLeftTail] = 0x8d30,
        [DraygonPhase.GoopLeftRecovery] = 0x8db2,
        [DraygonPhase.TryGrabSamus] = 0x8e19,
        [DraygonPhase.GrabbedSamus] = 0x8f10,
        [DraygonPhase.CarrySamus] = 0x8f1e,
        [DraygonPhase.FlailWithSamus] = 0x8fd6,
        [DraygonPhase.TailWhipWithSamus] = 0x90d4,
        [DraygonPhase.FinalTailWhips] = 0x9105,
        [DraygonPhase.FinalTailWhipsWait] = 0x9124,
        [DraygonPhase.ReleaseSamus] = 0x9128,
        [DraygonPhase.FlyStraightUp] = 0x9154,
        [DraygonPhase.Dying] = 0x9185,
        [DraygonPhase.DyingSink] = 0x9294,
        [DraygonPhase.DyingFinish] = 0x92ab,
    });

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
