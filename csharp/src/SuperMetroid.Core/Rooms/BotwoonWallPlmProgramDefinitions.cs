using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using Step = SuperMetroid.Core.Game.AnimationProgramStep<SuperMetroid.Core.Rooms.BotwoonWallProgram, SuperMetroid.Core.Rooms.BotwoonWallDraw, SuperMetroid.Core.Rooms.BotwoonWallCue>;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Botwoon's wall PLM programs (#1296), in place of their bank-$84 instruction lists at $84:AB31 and $84:AB67. The
/// crumble opens the first two scroll cells and crumbles the wall one row at a time, top to bottom.
/// </summary>
internal static class BotwoonWallPlmProgramDefinitions
{
    /// <summary>Rows the crumble program crumbles.</summary>
    internal const int CrumbleRows = 9;
    /// <summary>Updates each crumble frame lasts.</summary>
    internal const int CrumbleFrameDuration = 4;
    /// <summary>The sound each crumbled row queues: library two sound ten.</summary>
    internal static SoundEffectId CrumbleSound { get; } = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 10);
    /// <summary>The queue cap of the crumble sound.</summary>
    internal const byte CrumbleSoundMaximumQueued = 6;

    /// <summary>Every wall program.</summary>
    internal static AnimationProgramSet<BotwoonWallProgram, BotwoonWallDraw, BotwoonWallCue> Programs { get; } = new(
        new Dictionary<BotwoonWallProgram, Step[]>
        {
            [BotwoonWallProgram.Crumble] =
            [
                Step.SetCounter(CrumbleRows),
                Step.Publish(BotwoonWallCue.SetScrollsBlue),
                Step.FallInto(BotwoonWallProgram.CrumbleRow),
            ],
            [BotwoonWallProgram.CrumbleRow] =
            [
                Step.Publish(BotwoonWallCue.QueueCrumbleSound),
                Step.Show(BotwoonWallDraw.CrumbleFirst, CrumbleFrameDuration),
                Step.Show(BotwoonWallDraw.CrumbleSecond, CrumbleFrameDuration),
                Step.Show(BotwoonWallDraw.CrumbleThird, CrumbleFrameDuration),
                Step.Show(BotwoonWallDraw.CrumbleFourth, CrumbleFrameDuration),
                Step.Publish(BotwoonWallCue.MoveDownOneBlock),
                Step.RepeatWhileCounting(BotwoonWallProgram.CrumbleRow),
                Step.Stop,
            ],
            [BotwoonWallProgram.Clear] = [Step.Show(BotwoonWallDraw.Clear, 1), Step.Stop],
        });
}
