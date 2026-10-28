namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Selects the animation each of the three Tourian entrance-statue enemy records starts with.
/// </summary>
internal static class TourianEntranceStatues
{
    /// <summary>The animation the native even parameter zero, two or four selects ($AA:D7A5/D7AF/D7B9 table).</summary>
    internal static TourianEntranceStatueProgram InitialProgram(int parameter)
    {
        if (parameter > 4 || (parameter & 1) != 0)
            throw new ArgumentOutOfRangeException(nameof(parameter));
        return parameter switch
        {
            0 => TourianEntranceStatueProgram.BaseDecoration,
            2 => TourianEntranceStatueProgram.Ridley,
            _ => TourianEntranceStatueProgram.Phantoon,
        };
    }
}
