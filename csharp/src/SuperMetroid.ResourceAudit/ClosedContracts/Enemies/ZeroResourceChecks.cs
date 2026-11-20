using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms the identified Zero orientation-loop artwork omission.</summary>
internal static class ZeroResourceChecks
{
    /// <summary>Checks every Zero pose the orientation loops show against the installed compositions.</summary>
    public static void Run() => EnemyCompositionResourceChecks.RunSpritemaps("Zero", ZeroVisualDefinitions.Bank,
        EnemyCompositionSchemaTooling.Family("zero_").Select(frame => frame.Pointer),
        ZeroVisualDefinitions.FrameCount);
}
