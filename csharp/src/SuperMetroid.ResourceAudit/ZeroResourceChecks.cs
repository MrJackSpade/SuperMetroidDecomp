using SuperMetroid.Core.Game;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified Zero orientation-loop artwork omission.</summary>
internal static class ZeroResourceChecks
{
    public static void Run() => EnemyCompositionResourceChecks.Run("Zero", ZeroVisualDefinitions.Bank,
        Enumerable.Range(0, ZeroInstructionProgramDefinitions.PresentationWordCount)
            .Select(ZeroInstructionProgramDefinitions.PresentationWordAddress).ToArray(),
        ZeroVisualDefinitions.FrameCount, (int)EnemySpritemapSchema.PreZero,
        EnemySpritemapDefinitions.PreZeroFrameCount);
}
