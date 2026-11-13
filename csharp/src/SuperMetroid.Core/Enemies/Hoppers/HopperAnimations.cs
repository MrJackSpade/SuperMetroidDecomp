using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Hoppers.HopperCueKind;
using P = SuperMetroid.Core.Enemies.Hoppers.HopperSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The Sidehoppers' and Dessgeegas' floor and ceiling hop and landing animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="HopperCue"/> with its typed operands.
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
            Show(EnemyFrameId.SidehopperJumpFloor, 1),
        ],
        [P.SidehopperFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Land)),
            Show(EnemyFrameId.SidehopperFloorLandedFirst, 2),
            Show(EnemyFrameId.SidehopperFloorLandedSecond, 5),
            Show(EnemyFrameId.SidehopperFloorLandedFirst, 2),
            Show(EnemyFrameId.SidehopperFloorLandedSecond, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.SidehopperCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(EnemyFrameId.SidehopperJumpCeiling, 1),
        ],
        [P.SidehopperCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Land)),
            Show(EnemyFrameId.SidehopperCeilingLandedFirst, 2),
            Show(EnemyFrameId.SidehopperCeilingLandedSecond, 5),
            Show(EnemyFrameId.SidehopperCeilingLandedFirst, 2),
            Show(EnemyFrameId.SidehopperCeilingLandedSecond, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.DessgeegaFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.DessgeegaJumpFloor, 1),
        ],
        [P.DessgeegaFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.DessgeegaFloorLandedFirst, 2),
            Show(EnemyFrameId.DessgeegaFloorLandedSecond, 5),
            Show(EnemyFrameId.DessgeegaFloorLandedFirst, 2),
            Show(EnemyFrameId.DessgeegaFloorLandedSecond, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.DessgeegaCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.DessgeegaJumpCeiling, 1),
        ],
        [P.DessgeegaCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.DessgeegaCeilingLandedFirst, 2),
            Show(EnemyFrameId.DessgeegaCeilingLandedSecond, 5),
            Show(EnemyFrameId.DessgeegaCeilingLandedFirst, 2),
            Show(EnemyFrameId.DessgeegaCeilingLandedSecond, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.LargeSidehopperFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(EnemyFrameId.LargeSidehopperJumpFloor, 1),
        ],
        [P.LargeSidehopperFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Land)),
            Show(EnemyFrameId.LargeSidehopperFloorLandedFirst, 2),
            Show(EnemyFrameId.LargeSidehopperFloorLandedSecond, 5),
            Show(EnemyFrameId.LargeSidehopperFloorLandedFirst, 2),
            Show(EnemyFrameId.LargeSidehopperFloorLandedSecond, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.LargeSidehopperCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(EnemyFrameId.LargeSidehopperJumpCeiling, 1),
        ],
        [P.LargeSidehopperCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Land)),
            Show(EnemyFrameId.LargeSidehopperCeilingLandedFirst, 2),
            Show(EnemyFrameId.LargeSidehopperCeilingLandedSecond, 5),
            Show(EnemyFrameId.LargeSidehopperCeilingLandedFirst, 2),
            Show(EnemyFrameId.LargeSidehopperCeilingLandedSecond, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.LargeDessgeegaFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.LargeDessgeegaJumpFloor, 1),
        ],
        [P.LargeDessgeegaFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.LargeDessgeegaFloorLandedFirst, 2),
            Show(EnemyFrameId.LargeDessgeegaFloorLandedSecond, 5),
            Show(EnemyFrameId.LargeDessgeegaFloorLandedFirst, 2),
            Show(EnemyFrameId.LargeDessgeegaFloorLandedSecond, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.LargeDessgeegaCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.LargeDessgeegaJumpCeiling, 1),
        ],
        [P.LargeDessgeegaCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.LargeDessgeegaCeilingLandedFirst, 2),
            Show(EnemyFrameId.LargeDessgeegaCeilingLandedSecond, 5),
            Show(EnemyFrameId.LargeDessgeegaCeilingLandedFirst, 2),
            Show(EnemyFrameId.LargeDessgeegaCeilingLandedSecond, 3),
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
