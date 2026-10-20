using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Shitroid's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class ShitroidVisualDefinitions
{
    /// <summary>Shitroid instruction and ordinary OAM bank $A9.</summary>
    internal const byte Bank = 0xa9;
    /// <summary>Three Tourian baby Metroid drain and remorse compositions at $A9:F9A8..FAD8.</summary>
    internal const int FrameCount = 3;

    /// <summary>The spritemaps the typed Shitroid programs show, in program order.</summary>
    internal static ushort[] Spritemaps() => [.. Enum.GetValues<ShitroidProgram>()
        .SelectMany(program => ShitroidAnimations.Programs.Steps(program))
        .Where(step => step.Kind == AnimationProgramStepKind.Show)
        .Select(step => BabyMetroidVisualDefinitions.Spritemap(step.Frame.Index))];

    /// <summary>Names the three OAM compositions in first-shown order.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.FramesFromSpritemaps(Bank, Spritemaps(), FrameCount, "shitroid");
}

