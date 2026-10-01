using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Spark's ordinary artwork declarations; behavior and timing stay compiled.</summary>
internal static class SparkVisualDefinitions
{
    /// <summary>Spark instruction and ordinary OAM bank $A8.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>Eight Wrecked Ship Spark activation, active and emitter compositions at $A8:E71F..E79B.</summary>
    internal const int FrameCount = 8;

    internal static ushort[] Operands() => Enumerable.Range(0,
        SparkInstructionProgramDefinitions.PresentationWordCount)
        .Select(SparkInstructionProgramDefinitions.PresentationWordAddress).ToArray();

    internal static EnemySpritemapDefinition[] Frames() =>
        CompiledEnemyCompositionDefinitions.Frames(Bank, Operands(), FrameCount, "spark");
}

