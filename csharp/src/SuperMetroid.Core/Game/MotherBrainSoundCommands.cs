namespace SuperMetroid.Core.Game;

/// <summary>Native sound-library-one commands used by the Mother Brain drain sequence.</summary>
public static class MotherBrainSoundCommands
{
    /// <summary>$A9:BE96 queues library-one $02 when the rainbow drain runs out.</summary>
    public const ushort StopRainbowDrain = 0x0002;

    /// <summary>$A9:C889 queues library-one $40 when the Baby begins draining Mother Brain.</summary>
    public const ushort StartRainbowDrain = 0x0040;
}
