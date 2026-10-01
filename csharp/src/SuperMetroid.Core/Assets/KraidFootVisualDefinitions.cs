using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Kraid's independently animated extended foot compositions, not his BG2 body.</summary>
internal static class KraidFootVisualDefinitions
{
    /// <summary>Native Kraid instruction and artwork bank $A7.</summary>
    internal const byte Bank = 0xa7;
    /// <summary>Thirty-five selected roots: initial $A565 and walking/lunging/backwards $8CE3..8F47.</summary>
    internal const int FrameCount = 35;

    private static readonly EnemyExtendedFrameDefinition[] Definitions = Build();
    internal static ReadOnlySpan<EnemyExtendedFrameDefinition> Frames => Definitions;

    private static EnemyExtendedFrameDefinition[] Build()
    {
        var pointers = new SortedSet<ushort>();
        for (int index = 0; index < KraidFootInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = KraidFootInstructionProgramDefinitions.PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(Bank, operand, out ushort pointer))
                throw new InvalidDataException($"Kraid foot visual operand $A7:{operand:X4} is not compiled.");
            pointers.Add(pointer);
        }
        if (pointers.Count != FrameCount)
            throw new InvalidDataException("Kraid foot visual inventory changed.");
        return pointers.Select(pointer => new EnemyExtendedFrameDefinition(Bank, pointer,
            $"kraid_foot_oam_{pointer:X4}")).ToArray();
    }
}
