using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// One typed enemy-projectile family's oracle parameters, for a family that plays typed animations (#1600): its segments
/// are compared beat by beat, its animations with the lists' control flow, and a recording's native instruction pointer
/// translates to and from the playback it stands for.
/// </summary>
internal sealed class NativeEnemyProjectileAnimationFamily<TSegment, TFrame, TEvent>(
    NativeProgramEntry<TSegment>[] entries,
    AnimationSegments<TSegment, TFrame, TEvent> segments,
    IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
    Func<TFrame, ushort> presentationKey,
    NativeEnemyProjectileCueRules<TEvent> rules,
    Func<TFrame, ushort?>? reflectedPresentationKey,
    IReadOnlySet<TSegment>? startsWithClearedCounter) : NativeEnemyProjectileFamily
    where TSegment : struct, Enum
    where TFrame : struct
    where TEvent : notnull
{
    /// <inheritdoc/>
    internal override int CompareAll(IImportCartridgeSource cartridge) =>
        NativeEnemyProjectileProgramOracle.CompareAll(cartridge, entries, segments, startingAt, presentationKey, rules, reflectedPresentationKey,
            startsWithClearedCounter);

    /// <inheritdoc/>
    internal override Func<EnemyProjectileAnimation, ushort?> NativePointers(IImportCartridgeSource cartridge)
    {
        Dictionary<TSegment, NativeEnemyList<TSegment>> lists = Lists(cartridge);
        return animation => animation is EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent> typed
            ? NativeEnemyAnimationOracle.PointerOf(lists, entries, startingAt, typed.Playback)
            : null;
    }

    /// <inheritdoc/>
    internal override bool TryResumeAt(IImportCartridgeSource cartridge, EnemyProjectileAnimation animation, ushort nativePointer)
    {
        if (animation is not EnemyProjectileTypedAnimation<TSegment, TFrame, TEvent> typed)
            return false;
        typed.Playback = NativeEnemyAnimationOracle.PlaybackAt(Lists(cartridge), entries, startingAt, nativePointer);
        return true;
    }

    /// <summary>Every segment's native list, walked once per cartridge.</summary>
    private Dictionary<TSegment, NativeEnemyList<TSegment>> Lists(IImportCartridgeSource cartridge)
    {
        if (!ReferenceEquals(walkedCartridge, cartridge) || walkedLists is null)
        {
            walkedLists = NativeEnemyProjectileProgramOracle.Lists(cartridge, entries, segments, presentationKey, rules, reflectedPresentationKey);
            walkedCartridge = cartridge;
        }
        return walkedLists;
    }

    /// <summary>The cartridge <see cref="walkedLists"/> came from.</summary>
    private IImportCartridgeSource? walkedCartridge;

    /// <summary>The lists of the last walk.</summary>
    private Dictionary<TSegment, NativeEnemyList<TSegment>>? walkedLists;
}
