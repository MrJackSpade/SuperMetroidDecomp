using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// One typed enemy-projectile family's oracle parameters, for a family that plays typed animations (#1600): its segments
/// are compared beat by beat, its animations with the lists' control flow, and a recording's native instruction pointer
/// translates to and from the playback it stands for. Animations its owners start at native entries that only load the
/// counter (#2044) are compared and translated from those entries.
/// </summary>
internal sealed class NativeEnemyProjectileAnimationFamily<TSegment, TFrame, TEvent>(
    NativeProgramEntry<TSegment>[] entries,
    AnimationSegments<TSegment, TFrame, TEvent> segments,
    IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
    Func<TFrame, ushort> presentationKey,
    NativeEnemyProjectileCueRules<TEvent> rules,
    Func<TFrame, ushort?>? reflectedPresentationKey,
    IReadOnlySet<TSegment>? startsWithClearedCounter,
    IReadOnlyList<NativeCountedEntry<TSegment>> countedEntries,
    IReadOnlySet<TSegment>? onlyRunOnInto) : NativeEnemyProjectileFamily
    where TSegment : struct, Enum
    where TFrame : struct
    where TEvent : notnull
{
    /// <inheritdoc/>
    internal override int CompareAll(IImportCartridgeSource cartridge) =>
        NativeEnemyProjectileProgramOracle.CompareAll(cartridge, entries, segments, startingAt, presentationKey, rules, reflectedPresentationKey,
            startsWithClearedCounter, countedEntries, onlyRunOnInto);

    /// <inheritdoc/>
    internal override Func<EnemyProjectileAnimation, ushort?> NativePointers(IImportCartridgeSource cartridge)
    {
        (Dictionary<TSegment, NativeEnemyList<TSegment>> lists, IReadOnlyList<NativeCountedStart<TSegment>> counted) = Walk(cartridge);
        return animation => animation is EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent> typed
            ? NativeEnemyAnimationOracle.PointerOf(lists, entries, startingAt, typed.Playback, counted)
            : null;
    }

    /// <inheritdoc/>
    internal override bool TryResumeAt(IImportCartridgeSource cartridge, EnemyProjectileAnimation animation, ushort nativePointer, ushort nativeLoopCounter)
    {
        if (animation is not EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent> typed)
            return false;
        (Dictionary<TSegment, NativeEnemyList<TSegment>> lists, IReadOnlyList<NativeCountedStart<TSegment>> counted) = Walk(cartridge);
        typed.SetPlayback(NativeEnemyAnimationOracle.WithNativePlaysLeft(
            NativeEnemyAnimationOracle.PlaybackAt(lists, entries, startingAt, nativePointer, counted), nativeLoopCounter));
        return true;
    }

    /// <summary>
    /// The native entry of the list <paramref name="playback"/>'s phase plays: its segment's, or, for a segment the cartridge
    /// repeats under entries continuing elsewhere (#2376), the repeat the animation reaches the phase through.
    /// </summary>
    internal ushort ListEntryOf(IImportCartridgeSource cartridge, AnimationPlayback<TSegment> playback)
    {
        (Dictionary<TSegment, NativeEnemyList<TSegment>> lists, IReadOnlyList<NativeCountedStart<TSegment>> counted) = Walk(cartridge);
        return NativeEnemyAnimationOracle.ListPlayedBy(lists, startingAt, playback.Animation, playback.Place.Phase, counted).Entry;
    }

    /// <summary>Every segment's native list and every counted entry as its native list reads, walked once per cartridge.</summary>
    private (Dictionary<TSegment, NativeEnemyList<TSegment>> Lists, IReadOnlyList<NativeCountedStart<TSegment>> Counted) Walk(IImportCartridgeSource cartridge)
    {
        if (!ReferenceEquals(walkedCartridge, cartridge) || walked is null)
        {
            walked = (NativeEnemyProjectileProgramOracle.Lists(cartridge, entries, segments, presentationKey, rules, reflectedPresentationKey),
                NativeEnemyProjectileProgramOracle.CountedStarts(cartridge, entries, countedEntries, rules));
            walkedCartridge = cartridge;
        }
        return walked.Value;
    }

    /// <summary>The cartridge <see cref="walked"/> came from.</summary>
    private IImportCartridgeSource? walkedCartridge;

    /// <summary>The lists and counted entries of the last walk.</summary>
    private (Dictionary<TSegment, NativeEnemyList<TSegment>> Lists, IReadOnlyList<NativeCountedStart<TSegment>> Counted)? walked;
}
