namespace SuperMetroid.Core.Game;

/// <summary>Physical block-relative origin for one Mother Brain glass-shard emitter.</summary>
internal readonly record struct MotherBrainGlassShardPlacement(short XOffset, short YOffset);

/// <summary>Compiled animation selectors and physical origins for Mother Brain's glass shards.</summary>
internal static class MotherBrainGlassShardDefinitions
{
    /// <summary>Graphics index installed by the shard and sparkle initializers.</summary>
    internal const ushort GraphicsIndex = 0x0640;

    /// <summary>
    /// The three PLM-parameter-selected X/Y offset pairs at
    /// <c>$86:CE61-$86:CE6C</c>. Native parameters are even byte offsets zero, two, four.
    /// </summary>
    private static readonly MotherBrainGlassShardPlacement[] Placements =
    [
        new(8, 32),
        new(-40, 32),
        new(-16, 32),
    ];

    /// <summary>Returns the animation program selected by the RNG angle group.</summary>
    internal static ushort InstructionPointer(ushort animationIndex) =>
        MotherBrainGlassInstructionProgramDefinitions.SelectShardProgram(animationIndex);

    /// <summary>Returns the block-relative origin selected by an even native parameter.</summary>
    internal static MotherBrainGlassShardPlacement Placement(ushort parameter)
    {
        if (parameter is not (0 or 2 or 4))
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter,
                "Mother Brain glass-shard placement parameter must be zero, two, or four.");
        }

        return Placements[parameter >> 1];
    }
}
