using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Hoppers.HopperCueKind;
using P = SuperMetroid.Core.Enemies.Hoppers.HopperProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The Sidehoppers' and Dessgeegas' floor and ceiling hop and landing animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="HopperCue"/> with its typed operands.
/// </summary>
internal static class HopperAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<HopperProgram, EnemyFrameId, HopperCue> Segments { get; } = new(
        new Dictionary<HopperProgram, AnimationBeat<EnemyFrameId, HopperCue>[]>
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
            Show(EnemyFrameId.SidehopperLandFloor0, 2),
            Show(EnemyFrameId.SidehopperLandFloor1, 5),
            Show(EnemyFrameId.SidehopperLandFloor0, 2),
            Show(EnemyFrameId.SidehopperLandFloor1, 3),
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
            Show(EnemyFrameId.SidehopperLandCeiling0, 2),
            Show(EnemyFrameId.SidehopperLandCeiling1, 5),
            Show(EnemyFrameId.SidehopperLandCeiling0, 2),
            Show(EnemyFrameId.SidehopperLandCeiling1, 3),
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
            Show(EnemyFrameId.DessgeegaLandFloor0, 2),
            Show(EnemyFrameId.DessgeegaLandFloor1, 5),
            Show(EnemyFrameId.DessgeegaLandFloor0, 2),
            Show(EnemyFrameId.DessgeegaLandFloor1, 3),
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
            Show(EnemyFrameId.DessgeegaLandCeiling0, 2),
            Show(EnemyFrameId.DessgeegaLandCeiling1, 5),
            Show(EnemyFrameId.DessgeegaLandCeiling0, 2),
            Show(EnemyFrameId.DessgeegaLandCeiling1, 3),
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
            Show(EnemyFrameId.LargeSidehopperLandFloor0, 2),
            Show(EnemyFrameId.LargeSidehopperLandFloor1, 5),
            Show(EnemyFrameId.LargeSidehopperLandFloor0, 2),
            Show(EnemyFrameId.LargeSidehopperLandFloor1, 3),
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
            Show(EnemyFrameId.LargeSidehopperLandCeiling0, 2),
            Show(EnemyFrameId.LargeSidehopperLandCeiling1, 5),
            Show(EnemyFrameId.LargeSidehopperLandCeiling0, 2),
            Show(EnemyFrameId.LargeSidehopperLandCeiling1, 3),
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
            Show(EnemyFrameId.LargeDessgeegaLandFloor0, 2),
            Show(EnemyFrameId.LargeDessgeegaLandFloor1, 5),
            Show(EnemyFrameId.LargeDessgeegaLandFloor0, 2),
            Show(EnemyFrameId.LargeDessgeegaLandFloor1, 3),
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
            Show(EnemyFrameId.LargeDessgeegaLandCeiling0, 2),
            Show(EnemyFrameId.LargeDessgeegaLandCeiling1, 5),
            Show(EnemyFrameId.LargeDessgeegaLandCeiling0, 2),
            Show(EnemyFrameId.LargeDessgeegaLandCeiling1, 3),
            Cue(new(K.ReadyToHop)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<HopperProgram, Animation<HopperProgram>> StartingAt { get; } = new Dictionary<HopperProgram, Animation<HopperProgram>>
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
    internal static Animation<HopperProgram> StartingWith(HopperProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, HopperCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, HopperCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, HopperCue> Cue(HopperCue cue) => new AnimationBeat<EnemyFrameId, HopperCue>.Event(cue);
}
