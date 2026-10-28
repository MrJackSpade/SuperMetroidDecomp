namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Compiled fixed animation-program selectors for Waver.</summary>
internal static class WaverAnimationDefinitions
{
    /// <summary>
    /// The steady-left, steady-right, spinning-left and spinning-right animations the pointer table at
    /// <c>$A3:86DB-$A3:86E2</c> selects, by <see cref="WaverAnimationSelector"/>.
    /// </summary>
    internal static readonly Dictionary<WaverAnimationSelector, WaverProgram> Programs = new()
    {
        [WaverAnimationSelector.None] = WaverProgram.SteadyFacingLeft,
        [WaverAnimationSelector.FacingRight] = WaverProgram.SteadyFacingRight,
        [WaverAnimationSelector.Spinning] = WaverProgram.SpinningFacingLeft,
        [WaverAnimationSelector.Spinning | WaverAnimationSelector.FacingRight] = WaverProgram.SpinningFacingRight,
    };
}
