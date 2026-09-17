namespace SuperMetroid.Core.Game;

/// <summary>Compiled initialization selectors for the deactivated Work Robot.</summary>
internal static class WorkRobotInitializationDefinitions
{
    /// <summary>
    /// $A8:CC30..CC37: three authored instruction-list pointers followed by the
    /// first code word of MainAI_Robot. The native range check accepts parameter
    /// three, so that adjacent-code observation remains part of the definition.
    /// </summary>
    private static ReadOnlySpan<ushort> InitialInstructionLists =>
        [0xc6d3, 0xc6d9, 0xc6df, 0x54ae];

    /// <summary>Returns the native initial instruction word for sanitized parameter zero through three.</summary>
    internal static ushort GetInitialInstruction(int parameter)
    {
        if ((uint)parameter >= InitialInstructionLists.Length)
            throw new ArgumentOutOfRangeException(nameof(parameter));
        return InitialInstructionLists[parameter];
    }
}
