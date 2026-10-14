using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified Zero orientation-loop artwork omission.</summary>
internal static class ZeroResourceChecks
{
    /// <summary>Checks every Zero presentation-word selection against the PreZero spritemap resource domain.</summary>
    public static void Run() => EnemyCompositionResourceChecks.Run("Zero", ZeroVisualDefinitions.Bank,
        Enumerable.Range(0, ZeroInstructionProgramDefinitions.PresentationWordCount)
            .Select(ZeroInstructionProgramDefinitions.PresentationWordAddress).ToArray(),
        ZeroVisualDefinitions.FrameCount, (int)EnemySpritemapSchema.PreZero,
        EnemySpritemapDefinitions.PreZeroFrameCount);
}
