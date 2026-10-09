using SuperMetroid.Core.Game;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified Zero orientation-loop artwork omission.</summary>
internal static class ZeroResourceChecks
{
    /// <summary>Checks Zero's presentation operands against the finite installed artwork frames.</summary>
    public static void Run() => EnemyCompositionResourceChecks.Run("Zero", ZeroVisualDefinitions.Bank,
        Enumerable.Range(0, ZeroInstructionProgramDefinitions.PresentationWordCount)
            .Select(ZeroInstructionProgramDefinitions.PresentationWordAddress).ToArray(),
        ZeroVisualDefinitions.FrameCount, EnemySpritemapDefinitions.PreZeroVersion,
        EnemySpritemapDefinitions.PreZeroFrameCount);
}
