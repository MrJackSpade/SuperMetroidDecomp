using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The beats of a catalog of Mother Brain's brain animations whose own cues are <typeparamref name="TOwn"/> (#2554), and
/// the runs her head's catalogs share: her pictures, the cues every brain animation raises alike, and her four-ring volley
/// and idling from her damaged head.
/// </summary>
/// <typeparam name="TOwn">The cues only the catalog's animations raise.</typeparam>
internal static class MotherBrainBrainBeats<TOwn>
    where TOwn : struct, Enum
{
    /// <summary>
    /// Shows brain frame <paramref name="frame"/> for <paramref name="updates"/> updates (#2317), the updates it shows at
    /// her brain's draws.
    /// </summary>
    internal static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>> Show(EnemyFrameId frame, int updates) =>
        new AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>.Frame(frame, updates);

    /// <summary>Shows brain frame <paramref name="frame"/> for good: the still picture her brain rests on (#2318).</summary>
    internal static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>> Held(EnemyFrameId frame) =>
        new AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>.HeldFrame(frame);

    /// <summary>Raises a cue every brain animation raises alike that carries no values.</summary>
    internal static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>> Cue(MotherBrainHeadCueKind kind) =>
        Effect(MotherBrainHeadCue.Of(kind));

    /// <summary>Raises a cue every brain animation raises alike.</summary>
    internal static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>> Effect(MotherBrainHeadCue cue) =>
        new AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>.Event(new MotherBrainBrainCue<TOwn>.Shared(cue));

    /// <summary>Raises one of the catalog's own cues.</summary>
    internal static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>> Own(TOwn kind) =>
        new AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>.Event(new MotherBrainBrainCue<TOwn>.Own(kind));

    /// <summary>
    /// A volley's opening from <paramref name="head"/> (#2470): after <paramref name="beforeOpening"/> its mouth opens wide
    /// with <paramref name="cry"/> and, at its widest, fires the volley's first ring, <paramref name="ring"/>, whose sound
    /// the volley plays once. Phase two's volleys first hold her neck still and cry out; phase three's held it as they
    /// aimed and cry unless she has attacked the Baby eleven times.
    /// </summary>
    internal static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>[] OnionRingVolleyOpening(MotherBrainHeadMouth head,
        AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>[] beforeOpening, AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>> cry,
        AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>[] ring) =>
    [
        .. beforeOpening,
        Show(head.Open, 5),
        Show(head.Wide, 5),
        cry,
        Show(head.Widest, 9),
        .. ring,
        Effect(MotherBrainHeadCue.Sound(MotherBrainSoundDefinitions.OnionRings)),
    ];

    /// <summary>One more ring of a volley from <paramref name="head"/> (#2470): its widest mouth, then <paramref name="ring"/>.</summary>
    internal static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>[] OnionRingShot(MotherBrainHeadMouth head,
        AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>[] ring) =>
    [
        Show(head.Widest, 4),
        .. ring,
    ];

    /// <summary>
    /// A volley's end from <paramref name="head"/> (#2470): its widest mouth holds after the last ring, closes, and lets her
    /// neck move again.
    /// </summary>
    internal static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>[] OnionRingVolleyEnd(MotherBrainHeadMouth head) =>
    [
        Show(head.Widest, 17),
        Show(head.Wide, 5),
        Show(head.Open, 17),
        Cue(MotherBrainHeadCueKind.EnableNeckMovement),
    ];

    /// <summary>
    /// <paramref name="head"/> idling: its mouth opens and closes again. Phase two idles with the whole head, phase three
    /// with the damaged one.
    /// </summary>
    internal static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>[] Neutral(MotherBrainHeadMouth head) =>
    [
        Show(head.Closed, 5),
        Show(head.Opening, 5),
        Show(head.Open, 9),
        Show(head.Opening, 5),
        Show(head.Closed, 5),
        Show(head.Opening, 5),
    ];

    /// <summary>
    /// The damaged head holding its idling mouth open (#2554), whose <paramref name="hold"/> on a chance holds it again.
    /// </summary>
    internal static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>[] NeutralPhase3Hold(TOwn hold) =>
    [
        Show(EnemyFrameId.MotherBrainDarkHeadEyeGlaring, 9),
        Show(EnemyFrameId.MotherBrainDarkHeadEyeOpen, 9),
        Own(hold),
        Show(EnemyFrameId.MotherBrainDarkHeadEyeShut, 5),
    ];

    /// <summary>
    /// Her damaged head's four-ring volley after <paramref name="aim"/> (#2554): <paramref name="cry"/> as it opens unless
    /// she has attacked the Baby eleven times, each ring fired where the aim set it, then its idling with
    /// <paramref name="hold"/>.
    /// </summary>
    internal static Dictionary<TSegment, AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>[]> Phase3Volley<TSegment>(
        TSegment aiming, AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>[] aim,
        MotherBrainVolleySegments<TSegment> segments, TOwn cry, TOwn hold)
        where TSegment : struct, Enum => new()
    {
        [aiming] = aim,
        [segments.Opening] = OnionRingVolleyOpening(MotherBrainHeadMouth.Damaged, [], Own(cry), RingAimedBefore),
        [segments.Shot] = OnionRingShot(MotherBrainHeadMouth.Damaged, RingAimedBefore),
        [segments.End] = OnionRingVolleyEnd(MotherBrainHeadMouth.Damaged),
        [segments.Neutral] = Neutral(MotherBrainHeadMouth.Damaged),
        [segments.Hold] = NeutralPhase3Hold(hold),
    };

    /// <summary>A ring of phase three's volley: fired where her head aimed the volley, at Samus or the Baby, before it opened.</summary>
    internal static AnimationBeat<EnemyFrameId, MotherBrainBrainCue<TOwn>>[] RingAimedBefore => [Cue(MotherBrainHeadCueKind.SpawnOnionRing)];
}
