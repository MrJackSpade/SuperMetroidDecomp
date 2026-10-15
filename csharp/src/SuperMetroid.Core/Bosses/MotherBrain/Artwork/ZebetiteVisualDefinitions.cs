using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The presentation boundary of the Zebetites (#641): the bank-$A6 spritemaps of their health poses.</summary>
internal static class ZebetiteVisualDefinitions
{
    /// <summary>Zebetite ordinary OAM bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>Ten barrier health-tier compositions at $A6:FE08..FEB0.</summary>
    internal const int FrameCount = 10;

    /// <summary>The health pose spritemap at position <paramref name="index"/>: large tiers 0 to 4, split tiers 5 to 9.</summary>
    internal static ushort Spritemap(int index) => index switch
    {
        0 => 0xfe08,
        1 => 0xfe19,
        2 => 0xfe2a,
        3 => 0xfe4a,
        4 => 0xfe6a,
        5 => 0xfe8a,
        6 => 0xfe91,
        7 => 0xfe98,
        8 => 0xfea4,
        9 => 0xfeb0,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Zebetites have ten health poses."),
    };

    /// <summary>The spritemaps the typed health programs show, in program order.</summary>
    internal static ushort[] Spritemaps() => [.. Enum.GetValues<ZebetiteProgram>()
        .SelectMany(program => ZebetiteAnimations.Programs.Steps(program))
        .Where(step => step.Kind == AnimationProgramStepKind.Show)
        .Select(step => step.Frame.Spritemap)];

    /// <summary>Names the ten health compositions in program order.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.FramesFromSpritemaps(Bank, Spritemaps(), FrameCount, "zebetite");
}
