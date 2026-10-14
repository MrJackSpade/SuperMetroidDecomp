namespace SuperMetroid.Core.Game;

/// <summary>Compiled fixed animation-program selectors for Waver.</summary>
internal static class WaverAnimationDefinitions
{
    /// <summary>
    /// Steady-left, steady-right, spinning-left, and spinning-right instruction pointers
    /// at <c>$A3:86DB-$A3:86E2</c>, indexed by <see cref="WaverAnimationSelector"/>.
    /// </summary>
    internal static ushort InstructionList(WaverAnimationSelector selector) => selector switch
    {
        WaverAnimationSelector.None => WaverInstructionProgramDefinitions.SteadyFacingLeft,
        WaverAnimationSelector.FacingRight => WaverInstructionProgramDefinitions.SteadyFacingRight,
        WaverAnimationSelector.Spinning => WaverInstructionProgramDefinitions.SpinningFacingLeft,
        WaverAnimationSelector.Spinning | WaverAnimationSelector.FacingRight => WaverInstructionProgramDefinitions.SpinningFacingRight,
        _ => throw new InvalidDataException(
            $"Waver animation selector ${(int)selector:X4} exceeds its four-entry table."),
    };
}
