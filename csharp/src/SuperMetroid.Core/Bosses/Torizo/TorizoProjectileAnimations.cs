using SuperMetroid.Core.Animation;
using C = SuperMetroid.Core.Bosses.Torizo.TorizoFamilyCue<SuperMetroid.Core.Bosses.Torizo.TorizoFallCueKind>;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileCueKind;
using O = SuperMetroid.Core.Bosses.Torizo.TorizoFallCueKind;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;
using static SuperMetroid.Core.Bosses.Torizo.TorizoProjectileSide;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb and Golden Torizo's drool, explosive swipes, statue fragments, flying sonic booms and landing dust (#641, #1788,
/// #2011, #2554): each animation's steps, each frame the installed <see cref="EnemyProjectileFrameId"/> it shows and each
/// cue a shared <see cref="TorizoProjectileCue"/> or one of <see cref="TorizoFallCueKind"/>, which the behaviour that
/// starts the falls runs; the segments each animation plays whole, once or a number of times (#2235); and whether it then
/// loops or holds. A sonic boom and the landing dust are written once over the <see cref="TorizoProjectileSide"/> they
/// face or rise on (#2472). The Torizos' explosions, Chozo orbs, eggs and Golden Torizo's weapons are families of their
/// own, so each cue reaches only the behaviour that runs it.
/// </summary>
internal static class TorizoProjectileAnimations
{
    /// <summary>Every animation's steps.</summary>
    internal static AnimationSegments<TorizoProjectileSegment, EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>> Segments { get; } = new(
        new Dictionary<TorizoProjectileSegment, AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>>[]>
    {
        [P.DroolForming] = [Show(EnemyProjectileFrameId.Blank, 2)],
        [P.DroolNoDelay] =
        [
            Own(O.StartDroolFall),
            Cue(new TorizoProjectileCue.Signal(K.StopHarmingSamusAndDrawInFront)),
            Show(EnemyProjectileFrameId.TorizoDroolDrop, 5),
            Cue(new TorizoProjectileCue.Signal(K.DrawBehindSamus)),
        ],
        [P.DroolFalling] = [Hold(EnemyProjectileFrameId.TorizoDroolDrop)],
        [P.DroolWallImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.DroolFloorImpact] =
        [
            Cue(new TorizoProjectileCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.TorizoDroolSplashRising, 8),
            Show(EnemyProjectileFrameId.TorizoDroolSplashSpreading, 8),
            Show(EnemyProjectileFrameId.TorizoDroolSplashFlat, 8),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.ExplosiveSwipe] =
        [
            Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BombTorizoSwipe, Priority: SoundRequestPriority.WhenIdle)),
            Show(EnemyProjectileFrameId.TorizoSwipeSpark, 5),
            Show(EnemyProjectileFrameId.TorizoSwipeFlareBall, 5),
            Show(EnemyProjectileFrameId.TorizoSwipeFlareRingCored, 5),
            Show(EnemyProjectileFrameId.TorizoSwipeFlareRing, 5),
            Show(EnemyProjectileFrameId.TorizoSwipeFlareRingFading, 5),
            Cue(new TorizoProjectileCue.Signal(K.Delete)),
        ],
        [P.StatueFragmentTopMiddle] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueMossClump, restUpdates: 128),
        [P.StatueFragmentTopRight] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueRubbleLitRight, restUpdates: 120),
        [P.StatueFragmentMiddleLeft] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueRubbleLitCenter, restUpdates: 112),
        [P.StatueFragmentCentre] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueRubbleLitSpot, restUpdates: 104),
        [P.StatueFragmentMiddleRight] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueRubbleMossyTop, restUpdates: 96),
        [P.StatueFragmentBottomLeft] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueRubbleMossSpeck, restUpdates: 88),
        [P.StatueFragmentBottomMiddle] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueRubbleLitBase, restUpdates: 80),
        [P.StatueFragmentBottomRight] = StatueFragment(EnemyProjectileFrameId.BombTorizoStatueSlab, restUpdates: 72),
        [P.SonicBoomFiredLeft] = SonicBoomFired(Left),
        [P.SonicBoomFlyingLeft] = SonicBoomFlying(Left),
        [P.SonicBoomFiredRight] = SonicBoomFired(Right),
        [P.SonicBoomFlyingRight] = SonicBoomFlying(Right),
        [P.LandingDustRightFoot] = LandingDust(Right),
        [P.LandingDustLeftFoot] = LandingDust(Left),
    });

    /// <summary>
    /// The animation that starts at each segment a spawn, a reaction or a cue starts (#2235): the segments it plays whole,
    /// once or a number of times, and whether it then loops or holds. An animation that reaches a still picture holds it
    /// for good (#2353).
    /// </summary>
    internal static IReadOnlyDictionary<TorizoProjectileSegment, Animation<TorizoProjectileSegment>> StartingAt { get; } = new Dictionary<TorizoProjectileSegment, Animation<TorizoProjectileSegment>>
    {
        [P.DroolNoDelay] = new([Segments.Whole(P.DroolNoDelay), Segments.Whole(P.DroolFalling)], AnimationEnd.Holds),
        [P.DroolWallImpact] = new([Segments.Whole(P.DroolWallImpact)], AnimationEnd.Holds),
        [P.DroolFloorImpact] = new([Segments.Whole(P.DroolFloorImpact)], AnimationEnd.Holds),
        [P.ExplosiveSwipe] = new([Segments.Whole(P.ExplosiveSwipe)], AnimationEnd.Holds),
        [P.StatueFragmentTopMiddle] = new([Segments.Whole(P.StatueFragmentTopMiddle)], AnimationEnd.Holds),
        [P.StatueFragmentTopRight] = new([Segments.Whole(P.StatueFragmentTopRight)], AnimationEnd.Holds),
        [P.StatueFragmentMiddleLeft] = new([Segments.Whole(P.StatueFragmentMiddleLeft)], AnimationEnd.Holds),
        [P.StatueFragmentCentre] = new([Segments.Whole(P.StatueFragmentCentre)], AnimationEnd.Holds),
        [P.StatueFragmentMiddleRight] = new([Segments.Whole(P.StatueFragmentMiddleRight)], AnimationEnd.Holds),
        [P.StatueFragmentBottomLeft] = new([Segments.Whole(P.StatueFragmentBottomLeft)], AnimationEnd.Holds),
        [P.StatueFragmentBottomMiddle] = new([Segments.Whole(P.StatueFragmentBottomMiddle)], AnimationEnd.Holds),
        [P.StatueFragmentBottomRight] = new([Segments.Whole(P.StatueFragmentBottomRight)], AnimationEnd.Holds),
        [P.SonicBoomFiredLeft] = new([Segments.Whole(P.SonicBoomFiredLeft), Segments.Whole(P.SonicBoomFlyingLeft)], AnimationEnd.Holds),
        [P.SonicBoomFiredRight] = new([Segments.Whole(P.SonicBoomFiredRight), Segments.Whole(P.SonicBoomFlyingRight)], AnimationEnd.Holds),
        [P.LandingDustRightFoot] = new([Segments.Whole(P.LandingDustRightFoot)], AnimationEnd.Holds),
        [P.LandingDustLeftFoot] = new([Segments.Whole(P.LandingDustLeftFoot)], AnimationEnd.Holds),
    };

    /// <summary>How <paramref name="animation"/> plays.</summary>
    internal static Animation<TorizoProjectileSegment> StartingWith(TorizoProjectileSegment animation) => StartingAt[animation];

    /// <summary>
    /// A low-health drool that forms unseen for two updates before it is released and falls, holding its falling pose: one of
    /// the drools <see cref="TorizoRandomDefinitions.LowHealthDroolForm"/> chooses among.
    /// </summary>
    internal static Animation<TorizoProjectileSegment> DroolFormingForTwoUpdates { get; } =
        new([Segments.Whole(P.DroolForming), Segments.Whole(P.DroolNoDelay), Segments.Whole(P.DroolFalling)], AnimationEnd.Holds);

    /// <summary>A low-health drool that forms unseen for four updates, its forming played twice, before it is released and falls.</summary>
    internal static Animation<TorizoProjectileSegment> DroolFormingForFourUpdates { get; } =
        new([Segments.Repeated(P.DroolForming, 2), Segments.Whole(P.DroolNoDelay), Segments.Whole(P.DroolFalling)], AnimationEnd.Holds);

    /// <summary>A sonic boom thrown toward <paramref name="side"/>: its sound and its two growing poses.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>>[] SonicBoomFired(TorizoProjectileSide side) =>
    [
        Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.TorizoSonicBoom, Priority: SoundRequestPriority.Normal)),
        Show(side.SonicBoomSmall, 6),
        Show(side.SonicBoomMedium, 6),
    ];

    /// <summary>A sonic boom flying toward <paramref name="side"/>, at its full size.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>>[] SonicBoomFlying(TorizoProjectileSide side) =>
        [Hold(side.SonicBoomLarge)];

    /// <summary>
    /// The dust the Torizo's foot on <paramref name="side"/> raises on landing: its four puffs, each raising the dust a step,
    /// then its deletion.
    /// </summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>>[] LandingDust(TorizoProjectileSide side) =>
    [
        Show(side.DustPuff, 4),
        Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
        Show(side.DustPuffSmall, 4),
        Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
        Show(side.DustPuffThin, 4),
        Cue(new TorizoProjectileCue.Signal(K.RaiseDust)),
        Show(side.DustPuffFaint, 4),
        Cue(new TorizoProjectileCue.Signal(K.Delete)),
    ];

    /// <summary>
    /// A piece of the breaking statue hand (#2240), every piece alike: it shows <paramref name="fragment"/>, resting for
    /// <paramref name="restUpdates"/>, cracks off with the impact sound and starts its fall, falls for
    /// <see cref="BombTorizoStatueFragmentDefinitions.FallUpdates"/>, and is deleted.
    /// </summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>>[] StatueFragment(EnemyProjectileFrameId fragment, int restUpdates) =>
    [
        Show(fragment, restUpdates),
        Cue(new TorizoProjectileCue.PlaySound(Sound: EnemySound.BeamImpact, Priority: SoundRequestPriority.Normal)),
        Own(O.StartFragmentFall),
        Show(fragment, BombTorizoStatueFragmentDefinitions.FallUpdates),
        Cue(new TorizoProjectileCue.Signal(K.Delete)),
    ];

    /// <summary>Shows <paramref name="frame"/> for good: a still picture (#2235).</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>> Hold(EnemyProjectileFrameId frame) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>, which every Torizo projectile's behaviour runs alike.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>> Cue(TorizoProjectileCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>>.Event(new C.Shared(cue));

    /// <summary>Publishes <paramref name="kind"/>, which only the behaviour that starts their falls runs.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>> Own(TorizoFallCueKind kind) =>
        new AnimationBeat<EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>>.Event(new C.Own(kind));
}
