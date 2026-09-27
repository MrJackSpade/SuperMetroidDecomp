using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Installed body and wing compositions selected by KiHunter programs.</summary>
internal static class KiHunterVisualDefinitions
{
    /// <summary>$A8, the bank containing KiHunter's animation and OAM data.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>
    /// The 41 distinct body/wing spritemaps selected by 59 presentation operands
    /// in KiHunter's thirteen native programs. Addresses are stable frame identities;
    /// timing, callbacks and collision continue to use compiled mechanics.
    /// </summary>
    internal static EnemySpritemapDefinition[] Frames()
    {
        var pointers = new SortedSet<ushort>();
        for (int index = 0; index < KiHunterInstructionProgramDefinitions.PresentationWordCount;
             index++)
            pointers.Add(FrameAt(KiHunterInstructionProgramDefinitions.PresentationWordAddress(index)));
        return [.. pointers.Select(pointer => new EnemySpritemapDefinition(
            Bank, pointer, $"ki_hunter_a8_{pointer:x4}"))];
    }

    /// <summary>Accepts only compiled visual operands, never adjacent control words.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0; index < KiHunterInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            if (KiHunterInstructionProgramDefinitions.PresentationWordAddress(index) ==
                operandAddress &&
                CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort frame))
                return frame;
        }
        throw new InvalidDataException(
            $"KiHunter visual operand $A8:{operandAddress:X4} is not compiled.");
    }
}
