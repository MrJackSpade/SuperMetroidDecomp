using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Hoppers.HopperCueKind;
using P = SuperMetroid.Core.Enemies.Hoppers.HopperProgram;
using SuperMetroid.Core.Enemies.Hoppers.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The Sidehoppers' and Dessgeegas' floor and ceiling hop and landing animations as typed programs (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="HopperFrame"/> pose and each callback becomes a <see cref="HopperCue"/> with its typed operands.
/// </summary>
internal static class HopperAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<HopperProgram, HopperFrame, HopperCue> Programs { get; } = new(
        new Dictionary<HopperProgram, AnimationProgramStep<HopperProgram, HopperFrame, HopperCue>[]>
    {
        [P.SidehopperFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(2, 1),
            Hold,
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
            Hold,
        ],
        [P.SidehopperCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(5, 1),
            Hold,
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
            Hold,
        ],
        [P.DessgeegaFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(8, 1),
            Hold,
        ],
        [P.DessgeegaFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(6, 2),
            Show(7, 5),
            Show(6, 2),
            Show(7, 3),
            Cue(new(K.ReadyToHop)),
            Hold,
        ],
        [P.DessgeegaCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(11, 1),
            Hold,
        ],
        [P.DessgeegaCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(9, 2),
            Show(10, 5),
            Show(9, 2),
            Show(10, 3),
            Cue(new(K.ReadyToHop)),
            Hold,
        ],
        [P.LargeSidehopperFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(14, 1),
            Hold,
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
            Hold,
        ],
        [P.LargeSidehopperCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlaySound, Sound: HopperSoundDefinitions.Hop)),
            Show(17, 1),
            Hold,
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
            Hold,
        ],
        [P.LargeDessgeegaFloorJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(20, 1),
            Hold,
        ],
        [P.LargeDessgeegaFloorLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(18, 2),
            Show(19, 5),
            Show(18, 2),
            Show(19, 3),
            Cue(new(K.ReadyToHop)),
            Hold,
        ],
        [P.LargeDessgeegaCeilingJumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(23, 1),
            Hold,
        ],
        [P.LargeDessgeegaCeilingLanded] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(21, 2),
            Show(22, 5),
            Show(21, 2),
            Show(22, 3),
            Cue(new(K.ReadyToHop)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<HopperProgram, HopperFrame, HopperCue> Show(int pose, int duration) => AnimationProgramStep<HopperProgram, HopperFrame, HopperCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<HopperProgram, HopperFrame, HopperCue> Cue(HopperCue cue) => AnimationProgramStep<HopperProgram, HopperFrame, HopperCue>.Publish(cue);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<HopperProgram, HopperFrame, HopperCue> Hold => AnimationProgramStep<HopperProgram, HopperFrame, HopperCue>.Hold;

}
