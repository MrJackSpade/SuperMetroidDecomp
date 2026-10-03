namespace SuperMetroid.Core.Game;

/// <summary>Physical block-relative origin for one Mother Brain glass-shard emitter.</summary>
internal readonly record struct MotherBrainGlassShardPlacement(short XOffset, short YOffset);

/// <summary>Compiled animation selectors and physical origins for Mother Brain's glass shards.</summary>
internal static class MotherBrainGlassShardDefinitions
{
    /// <summary>Graphics index installed by the shard and sparkle initializers.</summary>
    internal const ushort GraphicsIndex = 0x0640;

    /// <summary>Returns the animation program selected by the RNG angle group.</summary>
    internal static ushort InstructionPointer(ushort animationIndex) =>
        MotherBrainGlassInstructionProgramDefinitions.SelectShardProgram(animationIndex);

    /// <summary>
    /// $86:CE61/CE67 select one of three horizontal emitter positions on the glass:
    /// right (+8), left (-40), or centre (-16), all two blocks below the PLM origin.
    /// Native parameters 0/2/4 are byte offsets, not a continuous motion sample.
    /// $86:CE08/CE16 add these signed offsets after scaling block coordinates by 16;
    /// the initializer then applies independent -8..7 jitter on each axis.
    /// </summary>
    internal static MotherBrainGlassShardPlacement Placement(ushort parameter)
    {
        if (parameter is not (0 or 2 or 4))
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter,
                "Mother Brain glass-shard placement parameter must be zero, two, or four.");
        }

        short x = parameter switch
        {
            0 => RightEmitterX,
            2 => LeftEmitterX,
            _ => CentreEmitterX,
        };
        return new(x, 2 * 16);
    }
    /// <summary>$86:CE61: right glass emitter, half a block right of the PLM origin.</summary>
    private const short RightEmitterX = 8;
    /// <summary>$86:CE63: left glass emitter, two and a half blocks left of the origin.</summary>
    private const short LeftEmitterX = -40;
    /// <summary>$86:CE65: centre glass emitter, one block left of the origin.</summary>
    private const short CentreEmitterX = -16;
}
