using SuperMetroid.Core.Fx;
using Step = SuperMetroid.Core.Fx.AnimationProgramStep<SuperMetroid.Core.Areas.Tourian.TourianAccessProgram, SuperMetroid.Core.Areas.Tourian.TourianAccessDraw, SuperMetroid.Core.Areas.Tourian.TourianAccessCue>;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The Tourian elevator access PLM programs (#1296), in place of their bank-$84 instruction lists: the crumble walks
/// down six rows, crumbling each through four images; the clear erases the access at once.
/// </summary>
internal static class TourianAccessPlmProgramDefinitions
{
    /// <summary>The rows the crumble walks down.</summary>
    internal const byte CrumbleRows = 6;
    /// <summary>How long each crumble image shows.</summary>
    internal const ushort CrumbleFrameDuration = 4;

    /// <summary>Every Tourian access program.</summary>
    internal static AnimationProgramSet<TourianAccessProgram, TourianAccessDraw, TourianAccessCue> Programs { get; } = new(
        new Dictionary<TourianAccessProgram, Step[]>
        {
            [TourianAccessProgram.Crumble] = [Step.SetCounter(CrumbleRows), Step.FallInto(TourianAccessProgram.CrumbleRow)],
            [TourianAccessProgram.CrumbleRow] =
            [
                Step.Show(TourianAccessDraw.CrumbleFirst, CrumbleFrameDuration),
                Step.Show(TourianAccessDraw.CrumbleSecond, CrumbleFrameDuration),
                Step.Show(TourianAccessDraw.CrumbleThird, CrumbleFrameDuration),
                Step.Show(TourianAccessDraw.EmptyRow, CrumbleFrameDuration),
                Step.Publish(TourianAccessCue.MoveDown),
                Step.RepeatWhileCounting(TourianAccessProgram.CrumbleRow),
                Step.Stop,
            ],
            [TourianAccessProgram.Clear] = [Step.Show(TourianAccessDraw.Clear, 1), Step.Stop],
        });
}
