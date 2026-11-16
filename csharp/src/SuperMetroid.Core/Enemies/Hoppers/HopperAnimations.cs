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
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(EnemyFrameId.SidehopperLegsStraight, 1),
        ],
        [P.SidehopperFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Land)),
            Show(EnemyFrameId.SidehopperLegsBent, 2),
            Show(EnemyFrameId.SidehopperLegsCrouched, 5),
            Show(EnemyFrameId.SidehopperLegsBent, 2),
            Show(EnemyFrameId.SidehopperLegsCrouched, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.SidehopperCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(EnemyFrameId.SidehopperUpsideDownLegsStraight, 1),
        ],
        [P.SidehopperCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Land)),
            Show(EnemyFrameId.SidehopperUpsideDownLegsBent, 2),
            Show(EnemyFrameId.SidehopperUpsideDownLegsCrouched, 5),
            Show(EnemyFrameId.SidehopperUpsideDownLegsBent, 2),
            Show(EnemyFrameId.SidehopperUpsideDownLegsCrouched, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.DessgeegaFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.DessgeegaLegsStraight, 1),
        ],
        [P.DessgeegaFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.DessgeegaLegsBent, 2),
            Show(EnemyFrameId.DessgeegaLegsCrouched, 5),
            Show(EnemyFrameId.DessgeegaLegsBent, 2),
            Show(EnemyFrameId.DessgeegaLegsCrouched, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.DessgeegaCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.DessgeegaUpsideDownLegsStraight, 1),
        ],
        [P.DessgeegaCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.DessgeegaUpsideDownLegsBent, 2),
            Show(EnemyFrameId.DessgeegaUpsideDownLegsCrouched, 5),
            Show(EnemyFrameId.DessgeegaUpsideDownLegsBent, 2),
            Show(EnemyFrameId.DessgeegaUpsideDownLegsCrouched, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.LargeSidehopperFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(EnemyFrameId.LargeSidehopperLegsStraight, 1),
        ],
        [P.LargeSidehopperFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Land)),
            Show(EnemyFrameId.LargeSidehopperLegsBent, 2),
            Show(EnemyFrameId.LargeSidehopperLegsCrouched, 5),
            Show(EnemyFrameId.LargeSidehopperLegsBent, 2),
            Show(EnemyFrameId.LargeSidehopperLegsCrouched, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.LargeSidehopperCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(EnemyFrameId.LargeSidehopperUpsideDownLegsStraight, 1),
        ],
        [P.LargeSidehopperCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Land)),
            Show(EnemyFrameId.LargeSidehopperUpsideDownLegsBent, 2),
            Show(EnemyFrameId.LargeSidehopperUpsideDownLegsCrouched, 5),
            Show(EnemyFrameId.LargeSidehopperUpsideDownLegsBent, 2),
            Show(EnemyFrameId.LargeSidehopperUpsideDownLegsCrouched, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.LargeDessgeegaFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.LargeDessgeegaLegsStraight, 1),
        ],
        [P.LargeDessgeegaFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.LargeDessgeegaLegsBent, 2),
            Show(EnemyFrameId.LargeDessgeegaLegsCrouched, 5),
            Show(EnemyFrameId.LargeDessgeegaLegsBent, 2),
            Show(EnemyFrameId.LargeDessgeegaLegsCrouched, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.LargeDessgeegaCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.LargeDessgeegaUpsideDownLegsStraight, 1),
        ],
        [P.LargeDessgeegaCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.LargeDessgeegaUpsideDownLegsBent, 2),
            Show(EnemyFrameId.LargeDessgeegaUpsideDownLegsCrouched, 5),
            Show(EnemyFrameId.LargeDessgeegaUpsideDownLegsBent, 2),
            Show(EnemyFrameId.LargeDessgeegaUpsideDownLegsCrouched, 3),
            Cue(new(K.ReadyToHop)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<HopperSegment, Animation<HopperSegment>> StartingAt { get; } = new Dictionary<HopperSegment, Animation<HopperSegment>>
    {
        [P.SidehopperFloorJumping] = new([new(P.SidehopperFloorJumping, 0, 3, 1)], AnimationEnd.Holds),
        [P.SidehopperFloorLanded] = new([new(P.SidehopperFloorLanded, 0, 7, 1)], AnimationEnd.Holds),
        [P.SidehopperCeilingJumping] = new([new(P.SidehopperCeilingJumping, 0, 3, 1)], AnimationEnd.Holds),
        [P.SidehopperCeilingLanded] = new([new(P.SidehopperCeilingLanded, 0, 7, 1)], AnimationEnd.Holds),
        [P.DessgeegaFloorJumping] = new([new(P.DessgeegaFloorJumping, 0, 2, 1)], AnimationEnd.Holds),
        [P.DessgeegaFloorLanded] = new([new(P.DessgeegaFloorLanded, 0, 6, 1)], AnimationEnd.Holds),
        [P.DessgeegaCeilingJumping] = new([new(P.DessgeegaCeilingJumping, 0, 2, 1)], AnimationEnd.Holds),
        [P.DessgeegaCeilingLanded] = new([new(P.DessgeegaCeilingLanded, 0, 6, 1)], AnimationEnd.Holds),
        [P.LargeSidehopperFloorJumping] = new([new(P.LargeSidehopperFloorJumping, 0, 3, 1)], AnimationEnd.Holds),
        [P.LargeSidehopperFloorLanded] = new([new(P.LargeSidehopperFloorLanded, 0, 7, 1)], AnimationEnd.Holds),
        [P.LargeSidehopperCeilingJumping] = new([new(P.LargeSidehopperCeilingJumping, 0, 3, 1)], AnimationEnd.Holds),
        [P.LargeSidehopperCeilingLanded] = new([new(P.LargeSidehopperCeilingLanded, 0, 7, 1)], AnimationEnd.Holds),
        [P.LargeDessgeegaFloorJumping] = new([new(P.LargeDessgeegaFloorJumping, 0, 2, 1)], AnimationEnd.Holds),
        [P.LargeDessgeegaFloorLanded] = new([new(P.LargeDessgeegaFloorLanded, 0, 6, 1)], AnimationEnd.Holds),
        [P.LargeDessgeegaCeilingJumping] = new([new(P.LargeDessgeegaCeilingJumping, 0, 2, 1)], AnimationEnd.Holds),
        [P.LargeDessgeegaCeilingLanded] = new([new(P.LargeDessgeegaCeilingLanded, 0, 6, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<HopperSegment> StartingWith(HopperSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, HopperCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, HopperCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, HopperCue> Cue(HopperCue cue) => new AnimationBeat<EnemyFrameId, HopperCue>.Event(cue);
}
