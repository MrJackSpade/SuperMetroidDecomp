using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified tatori-family extraction and display dependency.</summary>
internal static class MamaTurtleResourceChecks
{
    /// <summary>Checks the Mama/Baby Turtle display operands and frame count against the pre-extraction schema boundary.</summary>
    public static void Run() => EnemyCompositionResourceChecks.Run("Mama/Baby Turtle",
        MamaTurtleVisualDefinitions.Bank,
        Enumerable.Range(0, MamaTurtleInstructionProgramDefinitions.PresentationWordCount)
            .Select(MamaTurtleInstructionProgramDefinitions.PresentationWordAddress).ToArray(),
        MamaTurtleVisualDefinitions.FrameCount, (int)EnemySpritemapSchema.PreMamaTurtle,
        EnemySpritemapDefinitions.PreMamaTurtleFrameCount);
}
