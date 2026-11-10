using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Hoppers.HopperCueKind;
using P = SuperMetroid.Core.Enemies.Hoppers.HopperProgram;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The Sidehoppers' and Dessgeegas' floor and ceiling hop and landing animations as typed programs (#641). Each
/// frame shows a <see cref="HopperFrame"/> pose and each callback becomes a <see cref="HopperCue"/> with its typed operands.
/// </summary>
internal static class HopperAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<HopperProgram, HopperFrame, HopperCue> Segments { get; } = new(
        new Dictionary<HopperProgram, AnimationBeat<HopperFrame, HopperCue>[]>
    {
        [P.SidehopperFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(2, 1),
        ],
        [P.SidehopperFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Land)),
            Show(0, 2),
            Show(1, 5),
            Show(0, 2),
            Show(1, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.SidehopperCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(5, 1),
        ],
        [P.SidehopperCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Land)),
            Show(3, 2),
            Show(4, 5),
            Show(3, 2),
            Show(4, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.DessgeegaFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(8, 1),
        ],
        [P.DessgeegaFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(6, 2),
            Show(7, 5),
            Show(6, 2),
            Show(7, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.DessgeegaCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(11, 1),
        ],
        [P.DessgeegaCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(9, 2),
            Show(10, 5),
            Show(9, 2),
            Show(10, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.LargeSidehopperFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(14, 1),
        ],
        [P.LargeSidehopperFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Land)),
            Show(12, 2),
            Show(13, 5),
            Show(12, 2),
            Show(13, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.LargeSidehopperCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(17, 1),
        ],
        [P.LargeSidehopperCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Land)),
            Show(15, 2),
            Show(16, 5),
            Show(15, 2),
            Show(16, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.LargeDessgeegaFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(20, 1),
        ],
        [P.LargeDessgeegaFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(18, 2),
            Show(19, 5),
            Show(18, 2),
            Show(19, 3),
            Cue(new(K.ReadyToHop)),
        ],
        [P.LargeDessgeegaCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(23, 1),
        ],
        [P.LargeDessgeegaCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(21, 2),
            Show(22, 5),
            Show(21, 2),
            Show(22, 3),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<HopperFrame, HopperCue> Show(int pose, int duration) => new AnimationBeat<HopperFrame, HopperCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<HopperFrame, HopperCue> Cue(HopperCue cue) => new AnimationBeat<HopperFrame, HopperCue>.Event(cue);
}
