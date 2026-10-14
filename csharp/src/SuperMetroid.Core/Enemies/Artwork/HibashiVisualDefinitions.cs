using SuperMetroid.Core.Enemies.Hibashi;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Hibashi's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class HibashiVisualDefinitions
{
    /// <summary>Hibashi instruction and ordinary OAM bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The fire pillar's twenty-three selected OAM frames at $A6:9082..9469.</summary>
    internal const int FrameCount = 23;

    /// <summary>Collects presentation operands from Hibashi's compiled instruction lists so only referenced fire-pillar frames are imported.</summary>
    internal static ushort[] Operands() => Enumerable.Range(0,
        HibashiInstructionProgramDefinitions.PresentationWordCount)
        .Select(HibashiInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    /// <summary>Resolves the twenty-three bank-$A6 OAM compositions used for Hibashi's fire-pillar presentation.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "hibashi");
}

