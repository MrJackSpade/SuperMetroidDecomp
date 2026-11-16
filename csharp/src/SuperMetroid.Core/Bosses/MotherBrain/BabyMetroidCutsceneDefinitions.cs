namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid's health and waits through Mother Brain's last fight (#2216): its health as it arrives, through the
/// final charge and for the last volley, how long it waits before its dash and how long its fatal blow shakes it.
/// </summary>
internal static class BabyMetroidCutsceneDefinitions
{
    /// <summary>The Baby's health as it arrives; Mother Brain's onion rings wear it down while it shields Samus.</summary>
    internal const int ArrivalHealth = 3200;

    /// <summary>The count the Baby waits from before its dash onto the screen; it holds still one update longer.</summary>
    internal const int ArrivalWaitCount = 248;

    /// <summary>The health the Baby is given once Mother Brain's volleys have worn it out, carried into its last charge.</summary>
    internal const int FinalChargeHealth = 320;

    /// <summary>The health the Baby is given at the final charge's start, so the final four-ring volley kills it on its first hit.</summary>
    internal const int FinalVolleyHealth = 79;

    /// <summary>The count of the fatal blow's shake; the blow shakes the Baby one update longer.</summary>
    internal const int FatalBlowShakeCount = 16;

    /// <summary>
    /// Where the three dust clouds burst on Mother Brain's head as the Baby lets go, from her brain, in the order they
    /// spawn.
    /// </summary>
    internal static IReadOnlyList<Offset16<RoomPixels>> ReleaseDustOffsets { get; } = [new(-16, -8), new(0, -16), new(16, -8)];
}
