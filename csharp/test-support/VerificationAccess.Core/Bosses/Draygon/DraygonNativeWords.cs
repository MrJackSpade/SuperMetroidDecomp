using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Verification's native view of Draygon's identities (#1413): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class DraygonNativeWords
{
    /// <summary>
    /// The bank-$A5 routine word naming each kind of <see cref="DraygonPhase"/> after the fight begins, the body's record
    /// word A, and for a swoop or goop pass the way it travels (#2165), the cartridge giving each way a routine of its own.
    /// A swoop or goop phase is built with its swoop path (<see cref="PhaseOf"/>).
    /// </summary>
    internal static readonly NativeWordIdentities<(Type Kind, HorizontalDirection? Pass)> Phases =
        new(new Dictionary<(Type Kind, HorizontalDirection? Pass), ushort>
        {
            [(typeof(DraygonPhase.IntroInitialDelay), null)] = 0x871b,
            [(typeof(DraygonPhase.IntroDance), null)] = 0x878b,
            [(typeof(DraygonPhase.RightSwoopSetup), HorizontalDirection.Right)] = 0x87f4,
            [(typeof(DraygonPhase.SwoopDescending), HorizontalDirection.Right)] = 0x88b1,
            [(typeof(DraygonPhase.SwoopApex), HorizontalDirection.Right)] = 0x8922,
            [(typeof(DraygonPhase.SwoopAscending), HorizontalDirection.Right)] = 0x8951,
            [(typeof(DraygonPhase.LeftSwoopSetup), HorizontalDirection.Left)] = 0x89b3,
            [(typeof(DraygonPhase.SwoopDescending), HorizontalDirection.Left)] = 0x8a00,
            [(typeof(DraygonPhase.SwoopApex), HorizontalDirection.Left)] = 0x8a50,
            [(typeof(DraygonPhase.SwoopAscending), HorizontalDirection.Left)] = 0x8a90,
            [(typeof(DraygonPhase.GoopSetup), HorizontalDirection.Right)] = 0x8b0a,
            [(typeof(DraygonPhase.GoopApproach), HorizontalDirection.Right)] = 0x8b52,
            [(typeof(DraygonPhase.GoopTail), HorizontalDirection.Right)] = 0x8bae,
            [(typeof(DraygonPhase.GoopRecovery), HorizontalDirection.Right)] = 0x8c33,
            [(typeof(DraygonPhase.GoopSetup), HorizontalDirection.Left)] = 0x8c8e,
            [(typeof(DraygonPhase.GoopApproach), HorizontalDirection.Left)] = 0x8cd4,
            [(typeof(DraygonPhase.GoopTail), HorizontalDirection.Left)] = 0x8d30,
            [(typeof(DraygonPhase.GoopRecovery), HorizontalDirection.Left)] = 0x8db2,
            [(typeof(DraygonPhase.TryGrabSamus), null)] = 0x8e19,
            [(typeof(DraygonPhase.RepelGrapple), null)] = 0x8f10,
            [(typeof(DraygonPhase.CarrySamus), null)] = 0x8f1e,
            [(typeof(DraygonPhase.FlailWithSamus), null)] = 0x8fd6,
            [(typeof(DraygonPhase.TailWhipWithSamus), null)] = 0x90d4,
            [(typeof(DraygonPhase.FinalTailWhips), null)] = 0x9105,
            [(typeof(DraygonPhase.FinalTailWhipsWait), null)] = 0x9124,
            [(typeof(DraygonPhase.ReleaseSamus), null)] = 0x9128,
            [(typeof(DraygonPhase.FlyStraightUp), null)] = 0x9154,
            [(typeof(DraygonPhase.Dying), null)] = 0x9185,
            [(typeof(DraygonPhase.DyingWaitForEvirs), null)] = 0x9294,
            [(typeof(DraygonPhase.DyingFinish), null)] = 0x92ab,
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
    /// holds during a phase of <paramref name="kind"/>: the intro count before the first swoop, the burial wait while dying,
    /// and the swoop entry otherwise.
    /// </summary>
    internal static SharedValue MeaningOfSharedValue(Type kind) =>
        kind == typeof(DraygonPhase.IntroInitialDelay) || kind == typeof(DraygonPhase.IntroDance)
            ? SharedValue.IntroFrames
            : kind == typeof(DraygonPhase.Dying) || kind == typeof(DraygonPhase.DyingWaitForEvirs) || kind == typeof(DraygonPhase.DyingFinish)
                ? SharedValue.BurialWait
                : SharedValue.SwoopEntry;

    /// <summary>The swoop and goop phases, which carry the swoop path the last rightward swoop planned.</summary>
    private static readonly HashSet<Type> PathedKinds =
    [
        typeof(DraygonPhase.LeftSwoopSetup), typeof(DraygonPhase.SwoopDescending), typeof(DraygonPhase.SwoopApex),
        typeof(DraygonPhase.SwoopAscending), typeof(DraygonPhase.GoopSetup), typeof(DraygonPhase.GoopApproach),
        typeof(DraygonPhase.GoopTail), typeof(DraygonPhase.GoopRecovery),
    ];

    /// <summary>Whether a phase of <paramref name="kind"/> carries the swoop path.</summary>
    internal static bool CarriesSwoopPath(Type kind) => PathedKinds.Contains(kind);

    /// <summary>The phase of <paramref name="kind"/>, built with <paramref name="path"/> when it carries the swoop path.</summary>
    /// <exception cref="InvalidDataException">The phase carries the swoop path and the snapshot holds none.</exception>
    internal static DraygonPhase PhaseOf(Type kind, DraygonSwoopPath? path)
    {
        if (!CarriesSwoopPath(kind))
        {
            return kind == typeof(DraygonPhase.IntroInitialDelay) ? new DraygonPhase.IntroInitialDelay()
                : kind == typeof(DraygonPhase.IntroDance) ? new DraygonPhase.IntroDance()
                : kind == typeof(DraygonPhase.RightSwoopSetup) ? new DraygonPhase.RightSwoopSetup()
                : kind == typeof(DraygonPhase.TryGrabSamus) ? new DraygonPhase.TryGrabSamus()
                : kind == typeof(DraygonPhase.RepelGrapple) ? new DraygonPhase.RepelGrapple()
                : kind == typeof(DraygonPhase.CarrySamus) ? new DraygonPhase.CarrySamus()
                : kind == typeof(DraygonPhase.FlailWithSamus) ? new DraygonPhase.FlailWithSamus()
                : kind == typeof(DraygonPhase.BeginTailWhipWithSamus) ? new DraygonPhase.BeginTailWhipWithSamus()
                : kind == typeof(DraygonPhase.TailWhipWithSamus) ? new DraygonPhase.TailWhipWithSamus()
                : kind == typeof(DraygonPhase.FinalTailWhips) ? new DraygonPhase.FinalTailWhips()
                : kind == typeof(DraygonPhase.FinalTailWhipsWait) ? new DraygonPhase.FinalTailWhipsWait()
                : kind == typeof(DraygonPhase.ReleaseSamus) ? new DraygonPhase.ReleaseSamus()
                : kind == typeof(DraygonPhase.FlyStraightUp) ? new DraygonPhase.FlyStraightUp()
                : kind == typeof(DraygonPhase.Dying) ? new DraygonPhase.Dying()
                : kind == typeof(DraygonPhase.DyingWaitForEvirs) ? new DraygonPhase.DyingWaitForEvirs()
                : kind == typeof(DraygonPhase.DyingFinish) ? new DraygonPhase.DyingFinish()
                : throw new InvalidDataException($"Undefined Draygon phase kind {kind.Name}.");
        }
        DraygonSwoopPath swoop = path ?? throw new InvalidDataException($"A native {kind.Name} holds no swoop path.");
        return kind == typeof(DraygonPhase.LeftSwoopSetup) ? new DraygonPhase.LeftSwoopSetup(swoop)
            : kind == typeof(DraygonPhase.SwoopDescending) ? new DraygonPhase.SwoopDescending(swoop)
            : kind == typeof(DraygonPhase.SwoopApex) ? new DraygonPhase.SwoopApex(swoop)
            : kind == typeof(DraygonPhase.SwoopAscending) ? new DraygonPhase.SwoopAscending(swoop)
            : kind == typeof(DraygonPhase.GoopSetup) ? new DraygonPhase.GoopSetup(swoop)
            : kind == typeof(DraygonPhase.GoopApproach) ? new DraygonPhase.GoopApproach(swoop)
            : kind == typeof(DraygonPhase.GoopTail) ? new DraygonPhase.GoopTail(swoop)
            : new DraygonPhase.GoopRecovery(swoop);
    }
}
