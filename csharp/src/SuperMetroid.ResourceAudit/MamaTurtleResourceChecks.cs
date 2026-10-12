using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified tatori-family extraction and display dependency.</summary>
internal static class MamaTurtleResourceChecks
{
    public static void Run() => EnemyCompositionResourceChecks.Run("Mama/Baby Turtle",
        MamaTurtleVisualDefinitions.Bank,
        Enumerable.Range(0, MamaTurtleInstructionProgramDefinitions.PresentationWordCount)
            .Select(MamaTurtleInstructionProgramDefinitions.PresentationWordAddress).ToArray(),
        MamaTurtleVisualDefinitions.FrameCount, (int)EnemySpritemapSchema.PreMamaTurtle,
        EnemySpritemapDefinitions.PreMamaTurtleFrameCount);
}
