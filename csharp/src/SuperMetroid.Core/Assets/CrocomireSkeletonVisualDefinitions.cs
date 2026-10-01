using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>OAM composition roots for Crocomire's falling, collapsing, stable and river skeleton.</summary>
internal static class CrocomireSkeletonVisualDefinitions
{
    /// <summary>Crocomire's native visual bank $A4.</summary>
    internal const byte Bank = CrocomireBodyVisualDefinitions.Bank;
    /// <summary>Thirty-three corpse roots selected by the compiled bank-$A4 programs.</summary>
    internal const int FrameCount = 33;
    /// <summary>The collapse poses at $A4:E46A..E53E contain thirteen OAM components, not eight.</summary>
    internal const int MaximumComponents = 13;
    /// <summary><c>ExtendedSpritemap_CrocomireCorpse_E</c>, $A4:E46A: first thirteen-component pose.</summary>
    internal const ushort FirstThirteenComponentFrame = 0xe46a;

    private static readonly EnemyExtendedFrameDefinition[] Definitions = Build();
    internal static ReadOnlySpan<EnemyExtendedFrameDefinition> Frames => Definitions;

    internal static bool IsFrame(byte bank, ushort pointer) => bank == Bank &&
        Definitions.Any(frame => frame.Pointer == pointer);

    private static EnemyExtendedFrameDefinition[] Build()
    {
        var pointers = new SortedSet<ushort>();
        for (int index = 0; index < CrocomireInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = CrocomireInstructionProgramDefinitions.PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(Bank, operand, out ushort pointer))
                throw new InvalidDataException($"Crocomire visual operand $A4:{operand:X4} is not compiled.");
            if (pointer >= CrocomireBodyVisualDefinitions.FirstSkeletonFrame) pointers.Add(pointer);
        }
        if (pointers.Count != FrameCount)
            throw new InvalidDataException("Crocomire skeleton visual inventory changed.");
        return pointers.Select(pointer => new EnemyExtendedFrameDefinition(Bank, pointer,
            $"crocomire_skeleton_oam_{pointer:X4}")).ToArray();
    }
}
