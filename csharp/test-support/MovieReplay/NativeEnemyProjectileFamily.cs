using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// A typed enemy-projectile family beside its native bank-$86 lists: the ROM oracle compares its programs once, and
/// recordings translate its typed cursors to the native instruction pointers they hold.
/// </summary>
internal abstract class NativeEnemyProjectileFamily
{
    /// <summary>
    /// The family of typed animations <paramref name="startingAt"/> over <paramref name="segments"/> (#1600), whose native
    /// lists start at <paramref name="entries"/>; its owners start the counted lists of
    /// <paramref name="startsWithClearedCounter"/> directly, with a cleared counter, and the animations of
    /// <paramref name="countedEntries"/> at native entries that only load the counter (#2044); no owner starts the segments
    /// of <paramref name="onlyRunOnInto"/>, which animations only run on into (#2276).
    /// </summary>
    internal static NativeEnemyProjectileAnimationFamily<TSegment, TFrame, TEvent> Of<TSegment, TFrame, TEvent>(
        NativeProgramEntry<TSegment>[] entries,
        UnpausedAnimationSegments<TSegment, TFrame, TEvent> segments,
        IEnumerable<KeyValuePair<TSegment, Animation<TSegment>>> startingAt,
        Func<TFrame, ushort> presentationKey,
        NativeEnemyProjectileCueRules<TEvent> rules,
        Func<TFrame, ushort?>? reflectedPresentationKey = null,
        IReadOnlySet<TSegment>? startsWithClearedCounter = null,
        IReadOnlyList<NativeCountedEntry<TSegment>>? countedEntries = null,
        IReadOnlySet<TSegment>? onlyRunOnInto = null)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull =>
        new(entries, Ensure.NotNull(segments).Beats, Ensure.NotNull(startingAt).ToDictionary(pair => pair.Key, pair => pair.Value), presentationKey, rules,
            reflectedPresentationKey, startsWithClearedCounter, countedEntries ?? [], onlyRunOnInto);

    /// <summary>The oracle parameters of a family whose segments may hide the projectile, as <see cref="Of{TSegment, TFrame, TEvent}(NativeProgramEntry{TSegment}[], UnpausedAnimationSegments{TSegment, TFrame, TEvent}, IEnumerable{KeyValuePair{TSegment, Animation{TSegment}}}, Func{TFrame, ushort}, NativeEnemyProjectileCueRules{TEvent}, Func{TFrame, ushort?}?, IReadOnlySet{TSegment}?, IReadOnlyList{NativeCountedEntry{TSegment}}?, IReadOnlySet{TSegment}?)"/> gives them.</summary>
    internal static NativeEnemyProjectileAnimationFamily<TSegment, TFrame, TEvent> Of<TSegment, TFrame, TEvent>(
        NativeProgramEntry<TSegment>[] entries,
        HidingUnpausedAnimationSegments<TSegment, TFrame, TEvent> segments,
        IEnumerable<KeyValuePair<TSegment, Animation<TSegment>>> startingAt,
        Func<TFrame, ushort> presentationKey,
        NativeEnemyProjectileCueRules<TEvent> rules,
        Func<TFrame, ushort?>? reflectedPresentationKey = null,
        IReadOnlySet<TSegment>? startsWithClearedCounter = null,
        IReadOnlyList<NativeCountedEntry<TSegment>>? countedEntries = null,
        IReadOnlySet<TSegment>? onlyRunOnInto = null)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull =>
        new(entries, Ensure.NotNull(segments).Beats, Ensure.NotNull(startingAt).ToDictionary(pair => pair.Key, pair => pair.Value), presentationKey, rules,
            reflectedPresentationKey, startsWithClearedCounter, countedEntries ?? [], onlyRunOnInto);

    /// <summary>Compares every program with its native list and returns the total step count.</summary>
    internal abstract int CompareAll(IImportCartridgeSource cartridge);

    /// <summary>
    /// Walks the family's lists once and returns the native instruction pointer a typed animation of this family stands
    /// for, or null for an animation of another family.
    /// </summary>
    internal abstract Func<EnemyProjectileAnimation, ushort?> NativePointers(IImportCartridgeSource cartridge);

    /// <summary>
    /// Moves <paramref name="animation"/>, when it is this family's, to the typed step a recording's native instruction
    /// pointer <paramref name="nativePointer"/> stands for, with the plays its repeated phase has left read from the
    /// recording's loop counter <paramref name="nativeLoopCounter"/>; false for an animation of another family.
    /// </summary>
    internal abstract bool TryResumeAt(IImportCartridgeSource cartridge, EnemyProjectileAnimation animation, ushort nativePointer, ushort nativeLoopCounter);
}
