using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native figure-eight speed-phase word of Phantoon's body: the cartridge reads zero as the slow start, an odd word
/// as the surge and any other even word as settling, and writes zero, one and the surge's word plus one, two.
/// </summary>
internal static class PhantoonFigureEightStageWords
{
    /// <summary>The stage the cartridge's speed code reads <paramref name="word"/> as.</summary>
    internal static PhantoonFigureEightStage Decode(int word) =>
        word == 0 ? PhantoonFigureEightStage.EasingIn
        : (word & 1) != 0 ? PhantoonFigureEightStage.Surging
        : PhantoonFigureEightStage.Settling;

    /// <summary>The word the cartridge holds for <paramref name="stage"/> in the figure-eight it runs.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stage"/> is not a stage.</exception>
    internal static ushort Encode(PhantoonFigureEightStage stage) => stage switch
    {
        PhantoonFigureEightStage.EasingIn => 0,
        PhantoonFigureEightStage.Surging => 1,
        PhantoonFigureEightStage.Settling => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, "Undefined figure-eight stage."),
    };
}
