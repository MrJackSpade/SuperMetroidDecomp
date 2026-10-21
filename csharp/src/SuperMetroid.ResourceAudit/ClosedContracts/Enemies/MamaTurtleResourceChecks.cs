using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms the identified tatori-family extraction and display dependency.</summary>
internal static class MamaTurtleResourceChecks
{
    /// <summary>Checks the frames the Mama/Baby Turtle programs show and their count against the pre-extraction schema boundary.</summary>
    public static void Run() => EnemyCompositionResourceChecks.RunSpritemaps("Mama/Baby Turtle",
        MamaTurtleVisualDefinitions.Bank,
        MamaTurtleAnimationsTooling.Spritemaps(Enum.GetValues<MamaTurtleProgram>()),
        MamaTurtleVisualDefinitions.FrameCount, (int)EnemySpritemapSchema.PreMamaTurtle,
        EnemySpritemapDefinitions.PreMamaTurtleFrameCount);
}
