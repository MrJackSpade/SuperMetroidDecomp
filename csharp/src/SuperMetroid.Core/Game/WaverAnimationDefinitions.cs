namespace SuperMetroid.Core.Game;

/// <summary>
/// Composable Waver animation selector stored in variables C/F. The cartridge toggles
/// bit zero for facing and sets/clears bit one for the temporary spin family.
/// </summary>
[Flags]
public enum WaverAnimationSelector : ushort
{
    None = 0,
    FacingRight = 1,
    Spinning = 2,
}

/// <summary>Compiled fixed animation-program selectors for Waver.</summary>
internal static class WaverAnimationDefinitions
{
    /// <summary>
    /// Steady-left, steady-right, spinning-left, and spinning-right instruction pointers
    /// at <c>$A3:86DB-$A3:86E2</c>, indexed by <see cref="WaverAnimationSelector"/>.
    /// </summary>
    private static readonly ushort[] InstructionLists =
    [
        WaverInstructionProgramDefinitions.SteadyFacingLeft,
        WaverInstructionProgramDefinitions.SteadyFacingRight,
        WaverInstructionProgramDefinitions.SpinningFacingLeft,
        WaverInstructionProgramDefinitions.SpinningFacingRight,
    ];

    /// <summary>Returns the native instruction list for one facing/spin combination.</summary>
    internal static ushort InstructionList(WaverAnimationSelector selector)
    {
        int index = (int)selector;
        if ((uint)index >= InstructionLists.Length)
        {
            throw new InvalidDataException(
                $"Waver animation selector ${index:X4} exceeds its four-entry table.");
        }

        return InstructionLists[index];
    }
}
