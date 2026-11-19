using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Hoppers.HopperCueKind;
using P = SuperMetroid.Core.Enemies.Hoppers.HopperSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The Sidehoppers' and Dessgeegas' floor and ceiling hop and landing animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="HopperCue"/> with the values it acts on.
/// </summary>
internal static class HopperAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<HopperSegment, EnemyFrameId, HopperCue> Segments { get; } = new(
        new Dictionary<HopperSegment, AnimationBeat<EnemyFrameId, HopperCue>[]>
    {
        [P.SidehopperFloorJumping] =
        [
            Cue(new HopperCue.Signal(K.ProcessOffScreen)),
            Cue(new HopperCue.PlaySound(HopperSoundDefinitions.Hop)),
            Hold(EnemyFrameId.SidehopperLegsStraight),
        ],
        [P.SidehopperFloorLanded] =
        [
            Cue(new HopperCue.Signal(K.StopProcessingOffScreen)),
            Cue(new HopperCue.PlaySound(HopperSoundDefinitions.Land)),
            Show(EnemyFrameId.SidehopperLegsBent, 2),
            Show(EnemyFrameId.SidehopperLegsCrouched, 5),
            Show(EnemyFrameId.SidehopperLegsBent, 2),
            Show(EnemyFrameId.SidehopperLegsCrouched, 3),
            Cue(new HopperCue.Signal(K.ReadyToHop)),
        ],
        [P.SidehopperCeilingJumping] =
        [
            Cue(new HopperCue.Signal(K.ProcessOffScreen)),
            Cue(new HopperCue.PlaySound(HopperSoundDefinitions.Hop)),
            Hold(EnemyFrameId.SidehopperUpsideDownLegsStraight),
        ],
        [P.SidehopperCeilingLanded] =
        [
            Cue(new HopperCue.Signal(K.StopProcessingOffScreen)),
            Cue(new HopperCue.PlaySound(HopperSoundDefinitions.Land)),
            Show(EnemyFrameId.SidehopperUpsideDownLegsBent, 2),
            Show(EnemyFrameId.SidehopperUpsideDownLegsCrouched, 5),
            Show(EnemyFrameId.SidehopperUpsideDownLegsBent, 2),
            Show(EnemyFrameId.SidehopperUpsideDownLegsCrouched, 3),
            Cue(new HopperCue.Signal(K.ReadyToHop)),
        ],
        [P.DessgeegaFloorJumping] =
        [
            Cue(new HopperCue.Signal(K.ProcessOffScreen)),
            Hold(EnemyFrameId.DessgeegaLegsStraight),
        ],
        [P.DessgeegaFloorLanded] =
        [
            Cue(new HopperCue.Signal(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.DessgeegaLegsBent, 2),
            Show(EnemyFrameId.DessgeegaLegsCrouched, 5),
            Show(EnemyFrameId.DessgeegaLegsBent, 2),
            Show(EnemyFrameId.DessgeegaLegsCrouched, 3),
            Cue(new HopperCue.Signal(K.ReadyToHop)),
        ],
        [P.DessgeegaCeilingJumping] =
        [
            Cue(new HopperCue.Signal(K.ProcessOffScreen)),
            Hold(EnemyFrameId.DessgeegaUpsideDownLegsStraight),
        ],
        [P.DessgeegaCeilingLanded] =
        [
            Cue(new HopperCue.Signal(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.DessgeegaUpsideDownLegsBent, 2),
            Show(EnemyFrameId.DessgeegaUpsideDownLegsCrouched, 5),
            Show(EnemyFrameId.DessgeegaUpsideDownLegsBent, 2),
            Show(EnemyFrameId.DessgeegaUpsideDownLegsCrouched, 3),
            Cue(new HopperCue.Signal(K.ReadyToHop)),
        ],
        [P.LargeSidehopperFloorJumping] =
        [
            Cue(new HopperCue.Signal(K.ProcessOffScreen)),
            Cue(new HopperCue.PlaySound(HopperSoundDefinitions.Hop)),
            Hold(EnemyFrameId.LargeSidehopperLegsStraight),
        ],
        [P.LargeSidehopperFloorLanded] =
        [
            Cue(new HopperCue.Signal(K.StopProcessingOffScreen)),
            Cue(new HopperCue.PlaySound(HopperSoundDefinitions.Land)),
            Show(EnemyFrameId.LargeSidehopperLegsBent, 2),
            Show(EnemyFrameId.LargeSidehopperLegsCrouched, 5),
            Show(EnemyFrameId.LargeSidehopperLegsBent, 2),
            Show(EnemyFrameId.LargeSidehopperLegsCrouched, 3),
            Cue(new HopperCue.Signal(K.ReadyToHop)),
        ],
        [P.LargeSidehopperCeilingJumping] =
        [
            Cue(new HopperCue.Signal(K.ProcessOffScreen)),
            Cue(new HopperCue.PlaySound(HopperSoundDefinitions.Hop)),
            Hold(EnemyFrameId.LargeSidehopperUpsideDownLegsStraight),
        ],
        [P.LargeSidehopperCeilingLanded] =
        [
            Cue(new HopperCue.Signal(K.StopProcessingOffScreen)),
            Cue(new HopperCue.PlaySound(HopperSoundDefinitions.Land)),
            Show(EnemyFrameId.LargeSidehopperUpsideDownLegsBent, 2),
            Show(EnemyFrameId.LargeSidehopperUpsideDownLegsCrouched, 5),
            Show(EnemyFrameId.LargeSidehopperUpsideDownLegsBent, 2),
            Show(EnemyFrameId.LargeSidehopperUpsideDownLegsCrouched, 3),
            Cue(new HopperCue.Signal(K.ReadyToHop)),
        ],
        [P.LargeDessgeegaFloorJumping] =
        [
            Cue(new HopperCue.Signal(K.ProcessOffScreen)),
            Hold(EnemyFrameId.LargeDessgeegaLegsStraight),
        ],
        [P.LargeDessgeegaFloorLanded] =
        [
            Cue(new HopperCue.Signal(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.LargeDessgeegaLegsBent, 2),
            Show(EnemyFrameId.LargeDessgeegaLegsCrouched, 5),
            Show(EnemyFrameId.LargeDessgeegaLegsBent, 2),
            Show(EnemyFrameId.LargeDessgeegaLegsCrouched, 3),
            Cue(new HopperCue.Signal(K.ReadyToHop)),
        ],
        [P.LargeDessgeegaCeilingJumping] =
        [
            Cue(new HopperCue.Signal(K.ProcessOffScreen)),
            Hold(EnemyFrameId.LargeDessgeegaUpsideDownLegsStraight),
        ],
        [P.LargeDessgeegaCeilingLanded] =
        [
            Cue(new HopperCue.Signal(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.LargeDessgeegaUpsideDownLegsBent, 2),
            Show(EnemyFrameId.LargeDessgeegaUpsideDownLegsCrouched, 5),
            Show(EnemyFrameId.LargeDessgeegaUpsideDownLegsBent, 2),
            Show(EnemyFrameId.LargeDessgeegaUpsideDownLegsCrouched, 3),
            Cue(new HopperCue.Signal(K.ReadyToHop)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static Dictionary<HopperSegment, Animation<HopperSegment>> StartingAt { get; } = new Dictionary<HopperSegment, Animation<HopperSegment>>
    {
        [P.SidehopperFloorJumping] = new([Segments.Whole(P.SidehopperFloorJumping)], AnimationEnd.Holds),
        [P.SidehopperFloorLanded] = new([Segments.Whole(P.SidehopperFloorLanded)], AnimationEnd.Holds),
        [P.SidehopperCeilingJumping] = new([Segments.Whole(P.SidehopperCeilingJumping)], AnimationEnd.Holds),
        [P.SidehopperCeilingLanded] = new([Segments.Whole(P.SidehopperCeilingLanded)], AnimationEnd.Holds),
        [P.DessgeegaFloorJumping] = new([Segments.Whole(P.DessgeegaFloorJumping)], AnimationEnd.Holds),
        [P.DessgeegaFloorLanded] = new([Segments.Whole(P.DessgeegaFloorLanded)], AnimationEnd.Holds),
        [P.DessgeegaCeilingJumping] = new([Segments.Whole(P.DessgeegaCeilingJumping)], AnimationEnd.Holds),
        [P.DessgeegaCeilingLanded] = new([Segments.Whole(P.DessgeegaCeilingLanded)], AnimationEnd.Holds),
        [P.LargeSidehopperFloorJumping] = new([Segments.Whole(P.LargeSidehopperFloorJumping)], AnimationEnd.Holds),
        [P.LargeSidehopperFloorLanded] = new([Segments.Whole(P.LargeSidehopperFloorLanded)], AnimationEnd.Holds),
        [P.LargeSidehopperCeilingJumping] = new([Segments.Whole(P.LargeSidehopperCeilingJumping)], AnimationEnd.Holds),
        [P.LargeSidehopperCeilingLanded] = new([Segments.Whole(P.LargeSidehopperCeilingLanded)], AnimationEnd.Holds),
        [P.LargeDessgeegaFloorJumping] = new([Segments.Whole(P.LargeDessgeegaFloorJumping)], AnimationEnd.Holds),
        [P.LargeDessgeegaFloorLanded] = new([Segments.Whole(P.LargeDessgeegaFloorLanded)], AnimationEnd.Holds),
        [P.LargeDessgeegaCeilingJumping] = new([Segments.Whole(P.LargeDessgeegaCeilingJumping)], AnimationEnd.Holds),
        [P.LargeDessgeegaCeilingLanded] = new([Segments.Whole(P.LargeDessgeegaCeilingLanded)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<HopperSegment> StartingWith(HopperSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, HopperCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, HopperCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, HopperCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, HopperCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, HopperCue> Cue(HopperCue cue) => new AnimationBeat<EnemyFrameId, HopperCue>.Event(cue);
}
