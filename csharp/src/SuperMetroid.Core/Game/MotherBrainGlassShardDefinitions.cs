namespace SuperMetroid.Core.Game;

/// <summary>Physical block-relative origin for one Mother Brain glass-shard emitter.</summary>
internal readonly record struct MotherBrainGlassShardPlacement(short XOffset, short YOffset);

/// <summary>Compiled animation selectors and physical origins for Mother Brain's glass shards.</summary>
internal static class MotherBrainGlassShardDefinitions
{
    /// <summary>Graphics index installed by the shard and sparkle initializers.</summary>
    internal const ushort GraphicsIndex = 0x0640;

    /// <summary>
    /// Sixteen angle-group instruction selectors at <c>$86:CE41-$86:CE60</c>.
    /// Repeated pointers are retained because the RNG-derived index remains observable.
    /// </summary>
    private static readonly ushort[] InstructionPointers =
    [
        0xcc93,
        0xccb7,
        0xccb7,
        0xccdb,
        0xccdb,
        0xccdb,
        0xccff,
        0xccff,
        0xcd23,
        0xcd47,
        0xcd47,
        0xcd6b,
        0xcd6b,
        0xcd6b,
        0xcd8f,
        0xcd8f,
    ];

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
    internal static ushort InstructionPointer(ushort animationIndex)
    {
        if (animationIndex >= InstructionPointers.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(animationIndex), animationIndex,
                "Mother Brain glass-shard animation index must be zero through fifteen.");
        }

        return InstructionPointers[animationIndex];
    }

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
