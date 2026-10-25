using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The graphics and physical origins of Mother Brain's glass shards.</summary>
internal static class MotherBrainGlassShardDefinitions
{
    /// <summary>Graphics installed by the shard and sparkle initializers: from character $40, in palette row three.</summary>
    internal static SpriteGraphics Graphics { get; } = SpriteGraphics.Of(0x40, 3);

    /// <summary>
    /// One of three horizontal emitter positions on the glass: right (+8), left (-40), or centre (-16), all two blocks
    /// below the PLM origin. These signed offsets are added after scaling block coordinates by 16; the initializer then
    /// applies independent -8..7 jitter on each axis.
    /// </summary>
    internal static MotherBrainGlassShardPlacement Placement(MotherBrainGlassShard shard)
    {
        return new(new Offset16<RoomPixels>(shard switch
        {
            MotherBrainGlassShard.Right => RightEmitterX,
            MotherBrainGlassShard.Left => LeftEmitterX,
            MotherBrainGlassShard.Centre => CentreEmitterX,
            _ => throw new ArgumentOutOfRangeException(nameof(shard), shard, "Undefined Mother Brain glass emitter."),
        }, 2 * 16));
    }
    /// <summary>$86:CE61: right glass emitter, half a block right of the PLM origin.</summary>
    private const short RightEmitterX = 8;
    /// <summary>$86:CE63: left glass emitter, two and a half blocks left of the origin.</summary>
    private const short LeftEmitterX = -40;
    /// <summary>$86:CE65: centre glass emitter, one block left of the origin.</summary>
    private const short CentreEmitterX = -16;
}
