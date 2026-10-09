using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified tatori-family extraction and display dependency.</summary>
internal static class MamaTurtleResourceChecks
{
    /// <summary>Checks the identified Mama Turtle and baby-turtle visual operand family.</summary>
    public static void Run() => EnemyCompositionResourceChecks.Run("Mama/Baby Turtle",
        MamaTurtleVisualDefinitions.Bank,
        Enumerable.Range(0, MamaTurtleInstructionProgramDefinitions.PresentationWordCount)
            .Select(MamaTurtleInstructionProgramDefinitions.PresentationWordAddress).ToArray(),
        MamaTurtleVisualDefinitions.FrameCount, EnemySpritemapDefinitions.PreMamaTurtleVersion,
        EnemySpritemapDefinitions.PreMamaTurtleFrameCount);
}
