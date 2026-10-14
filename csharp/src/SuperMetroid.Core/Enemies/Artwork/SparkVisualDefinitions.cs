using SuperMetroid.Core.Enemies.Sparks;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Spark's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class SparkVisualDefinitions
{
    /// <summary>Spark instruction and ordinary OAM bank $A8.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>Eight Wrecked Ship Spark activation, active and emitter compositions at $A8:E71F..E79B.</summary>
    internal const int FrameCount = 8;

    /// <summary>Collects the spritemap operands from every compiled Spark presentation instruction.</summary>
    internal static ushort[] Operands() => Enumerable.Range(0,
        SparkInstructionProgramDefinitions.PresentationWordCount)
        .Select(SparkInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    /// <summary>Resolves the eight ordinary Spark compositions from their bank-$A8 operand words.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "spark");
}

