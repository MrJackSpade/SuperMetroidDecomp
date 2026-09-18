namespace SuperMetroid.Core.Game;

/// <summary>One authored Hibashi eruption hitbox frame.</summary>
internal readonly record struct HibashiActivityDefinition(
    ushort YOffset,
    ushort YRadius);

/// <summary>Compiled cartridge definitions for Hibashi/fire pillars.</summary>
internal static class HibashiDefinitions
{
    /// <summary>Enemy definition $E07F (Hibashi) in bank $A6.</summary>
    internal const ushort EnemyDefinition = 0xe07f;

    /// <summary>$A6:8D1B, instruction list for Hibashi's visible graphics part.</summary>
    internal const ushort GraphicsInstructionList = 0x8d1b;

    /// <summary>$A6:8DA9, instruction list for Hibashi's invisible collision part.</summary>
    internal const ushort HitboxInstructionList = 0x8da9;

    /// <summary>Sound effect $61 in library two, queued when an eruption begins.</summary>
    internal const ushort EruptionSoundEffect = 0x0061;

    /// <summary>
    /// $A6:8DBB/$A6:8DE7, the 22 eruption Y offsets and collision half-heights.
    /// </summary>
    private static readonly HibashiActivityDefinition[] ActivityFrames =
    [
        new(0x0005, 0x0018),
        new(0x000a, 0x0018),
        new(0x000f, 0x0018),
        new(0x0014, 0x0018),
        new(0x0019, 0x0018),
        new(0x001e, 0x0018),
        new(0x0023, 0x0018),
        new(0x0028, 0x0018),
        new(0x002d, 0x0018),
        new(0x0032, 0x0018),
        new(0x0037, 0x0018),
        new(0x003c, 0x0018),
        new(0x0041, 0x0018),
        new(0x0046, 0x0018),
        new(0x004b, 0x0018),
        new(0x0050, 0x0018),
        new(0x0055, 0x0018),
        new(0x005a, 0x0018),
        new(0x005f, 0x0014),
        new(0x0064, 0x0010),
        new(0x0069, 0x000c),
        new(0x006e, 0x0008),
    ];

    internal static HibashiActivityDefinition ActivityFrame(int index)
    {
        if ((uint)index >= ActivityFrames.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return ActivityFrames[index];
    }
}
