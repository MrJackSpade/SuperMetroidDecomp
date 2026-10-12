using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Powamp's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class PowampVisualDefinitions
{
    /// <summary>Powamp instruction and ordinary OAM bank $A8.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>Six Powamp body and balloon compositions at $A8:C675..C698.</summary>
    internal const int FrameCount = 6;

    internal static ushort[] Operands() => Enumerable.Range(0,
        PowampInstructionProgramDefinitions.PresentationWordCount)
        .Select(PowampInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "powamp");
}

