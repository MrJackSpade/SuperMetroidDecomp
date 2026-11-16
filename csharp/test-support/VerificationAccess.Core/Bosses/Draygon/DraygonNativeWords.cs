using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Verification's native view of Draygon's identities (#1413): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class DraygonNativeWords
{
    /// <summary>
    /// The bank-$A5 routine word naming each <see cref="DraygonPhase"/> after the fight begins, the body's record word A,
    /// and for a swoop or goop pass the way it travels (#2165), the cartridge giving each way a routine of its own.
    /// </summary>
    internal static readonly NativeWordIdentities<(DraygonPhase Phase, HorizontalDirection? Pass)> Phases =
        new(new Dictionary<(DraygonPhase Phase, HorizontalDirection? Pass), ushort>
        {
            [(DraygonPhase.IntroInitialDelay, null)] = 0x871b,
            [(DraygonPhase.IntroDance, null)] = 0x878b,
            [(DraygonPhase.SwoopSetup, HorizontalDirection.Right)] = 0x87f4,
            [(DraygonPhase.SwoopDescending, HorizontalDirection.Right)] = 0x88b1,
            [(DraygonPhase.SwoopApex, HorizontalDirection.Right)] = 0x8922,
            [(DraygonPhase.SwoopAscending, HorizontalDirection.Right)] = 0x8951,
            [(DraygonPhase.SwoopSetup, HorizontalDirection.Left)] = 0x89b3,
            [(DraygonPhase.SwoopDescending, HorizontalDirection.Left)] = 0x8a00,
            [(DraygonPhase.SwoopApex, HorizontalDirection.Left)] = 0x8a50,
            [(DraygonPhase.SwoopAscending, HorizontalDirection.Left)] = 0x8a90,
            [(DraygonPhase.GoopSetup, HorizontalDirection.Right)] = 0x8b0a,
            [(DraygonPhase.GoopApproach, HorizontalDirection.Right)] = 0x8b52,
            [(DraygonPhase.GoopTail, HorizontalDirection.Right)] = 0x8bae,
            [(DraygonPhase.GoopRecovery, HorizontalDirection.Right)] = 0x8c33,
            [(DraygonPhase.GoopSetup, HorizontalDirection.Left)] = 0x8c8e,
            [(DraygonPhase.GoopApproach, HorizontalDirection.Left)] = 0x8cd4,
            [(DraygonPhase.GoopTail, HorizontalDirection.Left)] = 0x8d30,
            [(DraygonPhase.GoopRecovery, HorizontalDirection.Left)] = 0x8db2,
            [(DraygonPhase.TryGrabSamus, null)] = 0x8e19,
            [(DraygonPhase.RepelGrapple, null)] = 0x8f10,
            [(DraygonPhase.CarrySamus, null)] = 0x8f1e,
            [(DraygonPhase.FlailWithSamus, null)] = 0x8fd6,
            [(DraygonPhase.TailWhipWithSamus, null)] = 0x90d4,
            [(DraygonPhase.FinalTailWhips, null)] = 0x9105,
            [(DraygonPhase.FinalTailWhipsWait, null)] = 0x9124,
            [(DraygonPhase.ReleaseSamus, null)] = 0x9128,
            [(DraygonPhase.FlyStraightUp, null)] = 0x9154,
            [(DraygonPhase.Dying, null)] = 0x9185,
            [(DraygonPhase.DyingWaitForEvirs, null)] = 0x9294,
            [(DraygonPhase.DyingFinish, null)] = 0x92ab,
        });

    /// <summary>
    /// The bank-$A5 routine word naming each <see cref="DraygonEyeAction"/> and, for a tracking eye, the way it faces
    /// (#2165).
    /// </summary>
    internal static readonly NativeWordIdentities<(DraygonEyeAction Action, HorizontalDirection? Facing)> EyeActions =
        new(new Dictionary<(DraygonEyeAction Action, HorizontalDirection? Facing), ushort>
        {
            [(DraygonEyeAction.Inert, null)] = 0x804b,
            [(DraygonEyeAction.Tracking, HorizontalDirection.Left)] = 0xc48d,
            [(DraygonEyeAction.Tracking, HorizontalDirection.Right)] = 0xc513,
        });

    /// <summary>The way the tracking eye <paramref name="word"/> names faces, or a failure naming <paramref name="meaning"/>.</summary>
    internal static HorizontalDirection TrackingFacing(int word, string meaning) =>
        EyeActions.Decode(word, meaning) is (DraygonEyeAction.Tracking, { } facing)
            ? facing
            : throw new InvalidDataException($"${word:X4} is not a tracking {meaning}.");

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
            : phase is DraygonPhase.Dying or DraygonPhase.DyingWaitForEvirs or DraygonPhase.DyingFinish
                ? SharedValue.BurialWait
                : SharedValue.SwoopEntry;
}
