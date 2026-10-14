namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Compiled fixed animation-program selectors for Waver.</summary>
internal static class WaverAnimationDefinitions
{
    /// <summary>
    /// The steady-left, steady-right, spinning-left and spinning-right animations the pointer table at
    /// <c>$A3:86DB-$A3:86E2</c> selects, indexed by <see cref="WaverAnimationSelector"/>.
    /// </summary>
    internal static WaverProgram Program(WaverAnimationSelector selector) => selector switch
    {
        WaverAnimationSelector.None => WaverProgram.SteadyFacingLeft,
        WaverAnimationSelector.FacingRight => WaverProgram.SteadyFacingRight,
        WaverAnimationSelector.Spinning => WaverProgram.SpinningFacingLeft,
        WaverAnimationSelector.Spinning | WaverAnimationSelector.FacingRight => WaverProgram.SpinningFacingRight,
        _ => throw new InvalidDataException(
            $"Waver animation selector ${(int)selector:X4} exceeds its four-entry table."),
    };
}
