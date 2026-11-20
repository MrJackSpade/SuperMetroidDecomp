using SuperMetroid.Core.Animation;
using C = SuperMetroid.Core.Bosses.Torizo.TorizoFamilyCue<SuperMetroid.Core.Bosses.Torizo.GoldenTorizoEggCueKind>;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileCueKind;
using O = SuperMetroid.Core.Bosses.Torizo.GoldenTorizoEggCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.GoldenTorizoEggSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The animations of Golden Torizo's eggs and their hatchlings (#2554): each animation's steps, each frame the installed
/// <see cref="EnemyProjectileFrameId"/> it shows and each cue a shared <see cref="TorizoProjectileCue"/> or one of
/// <see cref="GoldenTorizoEggCueKind"/>, which only the egg behaviour runs; the segments each animation plays whole, once or a number
/// of times (#2235); and whether it then loops or holds.
/// </summary>
internal static class GoldenTorizoEggAnimations
{
    /// <summary>Every animation's steps.</summary>
    internal static AnimationSegments<GoldenTorizoEggSegment, EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>> Segments { get; } = new(
        new Dictionary<GoldenTorizoEggSegment, AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>>[]>
    {
        [P.EggBouncingLeft] = EggBouncing(Left),
        [P.EggHatchingLeft] = EggHatching(Left),
        [P.EggBouncingRight] = EggBouncing(Right),
        [P.EggHatchingRight] = EggHatching(Right),
        [P.EggHatch] =
        [
            Cue(new TorizoProjectileCue.Signal(K.TurnHarmful)),
            Cue(new TorizoProjectileCue.Signal(K.BlockShots)),
            Own(O.Hatch),
        ],
        [P.EggHatchedLeft] = [.. EggHatched],
        [P.HatchlingChargingLeft] = HatchlingCharging(Left),
        [P.EggHatchedRight] = [.. EggHatched],
        [P.HatchlingChargingRight] = HatchlingCharging(Right),
        [P.EggBreakLeft] = EggBreak(Left, remainsFlatUpdates: 10),
        [P.EggBreakRight] = EggBreak(Right, remainsFlatUpdates: 8),
        [P.Shot] =
        [
            .. ChozoOrbBursting,
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment a spawn, a reaction or a cue starts (#2235): the segments it plays whole,
    /// once or a number of times, and whether it then loops or holds. An animation that reaches a still picture holds it
    /// for good (#2353).
    /// </summary>
    internal static IReadOnlyDictionary<GoldenTorizoEggSegment, Animation<GoldenTorizoEggSegment>> StartingAt { get; } = new Dictionary<GoldenTorizoEggSegment, Animation<GoldenTorizoEggSegment>>
    {
        [P.EggBouncingLeft] = new([Segments.Whole(P.EggBouncingLeft)], AnimationEnd.Holds),
        [P.EggHatchingLeft] = new([Segments.Whole(P.EggHatchingLeft), Segments.Whole(P.EggHatch)], AnimationEnd.Holds),
        [P.EggBouncingRight] = new([Segments.Whole(P.EggBouncingRight)], AnimationEnd.Holds),
        [P.EggHatchingRight] = new([Segments.Whole(P.EggHatchingRight), Segments.Whole(P.EggHatch)], AnimationEnd.Holds),
        [P.EggHatchedLeft] = Animation<P>.LeadingInto([Segments.Whole(P.EggHatchedLeft)], [Segments.Whole(P.HatchlingChargingLeft)]),
        [P.EggHatchedRight] = Animation<P>.LeadingInto([Segments.Whole(P.EggHatchedRight)], [Segments.Whole(P.HatchlingChargingRight)]),
        [P.EggBreakLeft] = new([Segments.Whole(P.EggBreakLeft)], AnimationEnd.Holds),
        [P.EggBreakRight] = new([Segments.Whole(P.EggBreakRight)], AnimationEnd.Holds),
        [P.Shot] = new([Segments.Whole(P.Shot)], AnimationEnd.Holds),
    };

    /// <summary>How <paramref name="animation"/> plays.</summary>
    internal static Animation<GoldenTorizoEggSegment> StartingWith(GoldenTorizoEggSegment animation) => StartingAt[animation];

    /// <summary>A cue's continuation into <paramref name="animation"/>, from its start.</summary>
    internal static AnimationContinuation<GoldenTorizoEggSegment> ContinueWith(GoldenTorizoEggSegment animation) =>
        new(StartingWith(animation), AnimationPlace.StartOfAnimation);

    /// <summary>
    /// A Chozo orb bursting (#2411), on a wall or shot: it stops, survives touching Samus in front of her, and plays its five
    /// poses; a shot orb's burst then drops its pickup.
    /// </summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>>[] ChozoOrbBursting =>
    [
        Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
        Cue(new TorizoProjectileCue.Signal(K.PersistOnContactAndDrawInFront)),
        Show(EnemyProjectileFrameId.TorizoChozoOrbSpark, 4),
        Show(EnemyProjectileFrameId.TorizoChozoOrbFlareBall, 4),
        Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingCored, 4),
        Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRing, 4),
        Show(EnemyProjectileFrameId.TorizoChozoOrbFlareRingFading, 4),
    ];

    /// <summary>An egg facing <paramref name="side"/>, whole as it bounces.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>>[] EggBouncing(TorizoProjectileSide side) =>
        [Hold(side.EggWhole)];

    /// <summary>An egg facing <paramref name="side"/> hatching: it stops, cracks, splits and opens.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>>[] EggHatching(TorizoProjectileSide side) =>
    [
        Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
        Show(side.EggCracked, 4),
        Show(side.EggSplitting, 4),
        Show(side.EggHatching, 4),
    ];

    /// <summary>A hatched egg's creature, either way it faces: it squeaks and starts its charge.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>>[] EggHatched =>
    [
        Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.RisingSqueaks, Priority: SoundRequestPriority.Normal)),
        Own(O.StartEggCharge),
    ];

    /// <summary>A hatchling charging toward <paramref name="side"/>: claws raised, lowered, hunched and lowered again.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>>[] HatchlingCharging(TorizoProjectileSide side) =>
    [
        Show(side.HatchlingClawsRaised, 6),
        Show(side.HatchlingClawsLowered, 6),
        Show(side.HatchlingHunched, 6),
        Show(side.HatchlingClawsLowered, 6),
    ];

    /// <summary>
    /// A shot hatchling facing <paramref name="side"/> breaking: it stops, crumbles, collapses, slumps and lies in its
    /// remains, flattened for <paramref name="remainsFlatUpdates"/>, then is deleted.
    /// </summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>>[] EggBreak(TorizoProjectileSide side, int remainsFlatUpdates) =>
    [
        Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
        Show(side.HatchlingCrumbling, 4),
        Show(side.HatchlingCollapsing, 4),
        Show(side.HatchlingSlumped, 4),
        Show(side.HatchlingRemains, 4),
        Show(side.HatchlingRemainsFlat, remainsFlatUpdates),
        Cue(new TorizoProjectileCue.Signal(K.Delete)),
    ];

    /// <summary>Shows <paramref name="frame"/> for good: a still picture (#2235).</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>> Hold(EnemyProjectileFrameId frame) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>, which every Torizo projectile's behaviour runs alike.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>> Cue(TorizoProjectileCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>>.Event(new C.Shared(cue));

    /// <summary>Publishes <paramref name="kind"/>, which only the egg behaviour runs.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>> Own(GoldenTorizoEggCueKind kind) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<GoldenTorizoEggCueKind>>.Event(new C.Own(kind));
}
