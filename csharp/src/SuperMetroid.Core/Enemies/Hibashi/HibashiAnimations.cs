using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Hibashi.HibashiCueKind;
using P = SuperMetroid.Core.Enemies.Hibashi.HibashiSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The Hibashi fire pillar's erupting flame and its collision part as typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="HibashiCue"/> with the values it acts on.
/// </summary>
internal static class HibashiAnimations
{
    /// <summary>How far the flame's top rises with each step of its eruption, in pixels.</summary>
    private const int EruptionRise = 5;

    /// <summary>The eruption's pictures from the ember to the flame's top vanishing, each with how long it shows.</summary>
    private static readonly (EnemyFrameId Picture, int Updates)[] EruptionPictures =
    [
        (EnemyFrameId.HibashiEmber, 2),
        (EnemyFrameId.HibashiFlameBursting, 2),
        (EnemyFrameId.HibashiFlameSparking, 2),
        (EnemyFrameId.HibashiFlameSpitting, 2),
        (EnemyFrameId.HibashiFlameChurning, 1),
        (EnemyFrameId.HibashiFlameClimbing, 1),
        (EnemyFrameId.HibashiFlameSplashing, 1),
        (EnemyFrameId.HibashiFlameSpraying, 1),
        (EnemyFrameId.HibashiFlameSwirling, 2),
        (EnemyFrameId.HibashiFlameLifting, 2),
        (EnemyFrameId.HibashiFlameTrailing, 2),
        (EnemyFrameId.HibashiFlameStreaming, 2),
        (EnemyFrameId.HibashiFlameFlaring, 2),
        (EnemyFrameId.HibashiFlameSoaring, 2),
        (EnemyFrameId.HibashiFlameReaching, 2),
        (EnemyFrameId.HibashiFlameFullHeight, 2),
        (EnemyFrameId.HibashiFlameFullHeightSparking, 4),
        (EnemyFrameId.HibashiFlameFullHeightCharring, 4),
        (EnemyFrameId.HibashiFlameFullHeightGuttering, 4),
        (EnemyFrameId.HibashiFlameTopBreakingUp, 4),
        (EnemyFrameId.HibashiFlameTopScattering, 4),
        (EnemyFrameId.HibashiFlameTopThinning, 4),
        (EnemyFrameId.HibashiFlameTopVanishing, 4),
    ];

    /// <summary>
    /// The flame's hit area's half-height after each step of its rise: its full 24 pixels until its top breaks up, then
    /// shrinking as the top scatters, thins and vanishes.
    /// </summary>
    private static readonly int[] EruptionHalfHeights = [.. DefinitionRun.Repeated(24, 18), 20, 16, 12, 8];

    /// <summary>
    /// Every segment's beats. The hidden collision part's held frame is never drawn, but an enemy showing no frame takes
    /// no part in Samus's collision, so its hold is what admits the part's hit area from its first update on (#2566).
    /// </summary>
    internal static AnimationSegments<HibashiSegment, EnemyFrameId, HibashiCue> Segments { get; } = new(
        new Dictionary<HibashiSegment, AnimationBeat<EnemyFrameId, HibashiCue>[]>
    {
        [P.Flame] = Eruption,
        [P.Hitbox] =
        [
            Hold(EnemyFrameId.HibashiEmber),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static Dictionary<HibashiSegment, Animation<HibashiSegment>> StartingAt { get; } = new Dictionary<HibashiSegment, Animation<HibashiSegment>>
    {
        [P.Flame] = new([Segments.Whole(P.Flame)], AnimationEnd.Holds),
        [P.Hitbox] = new([Segments.Whole(P.Hitbox)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<HibashiSegment> StartingWith(HibashiSegment segment) => StartingAt[segment];

    /// <summary>
    /// The flame's eruption: its sound, then each of <see cref="EruptionPictures"/>, the flame's top rising
    /// <see cref="EruptionRise"/> pixels with each step between them, its hit area as tall as <see cref="EruptionHalfHeights"/>
    /// says and its width restored at the first step, and the activity finishing after the last.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, HibashiCue>[] Eruption =>
    [
        Cue(new HibashiCue.Signal(K.PlayEruptionSound)),
        Show(EruptionPictures[0].Picture, EruptionPictures[0].Updates),
        Cue(new HibashiCue.EruptionStep(new(YOffset: EruptionRise, YRadius: EruptionHalfHeights[0], HibashiStepWidth.Restored))),
        .. Enumerable.Range(1, EruptionHalfHeights.Length - 1).SelectMany(step => new[]
        {
            Show(EruptionPictures[step].Picture, EruptionPictures[step].Updates),
            Cue(new HibashiCue.EruptionStep(new(YOffset: EruptionRise * (step + 1), YRadius: EruptionHalfHeights[step]))),
        }),
        Show(EruptionPictures[^1].Picture, EruptionPictures[^1].Updates),
        Cue(new HibashiCue.Signal(K.FinishActivity)),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, HibashiCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, HibashiCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): the collision part's still picture rests on it.</summary>
    private static AnimationBeat<EnemyFrameId, HibashiCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, HibashiCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, HibashiCue> Cue(HibashiCue cue) => new AnimationBeat<EnemyFrameId, HibashiCue>.Event(cue);
}
