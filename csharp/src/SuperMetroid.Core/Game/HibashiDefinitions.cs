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
    internal const ushort GraphicsInstructionList =
        HibashiInstructionProgramDefinitions.GraphicsProgram;

    /// <summary>$A6:8DA9, instruction list for Hibashi's invisible collision part.</summary>
    internal const ushort HitboxInstructionList =
        HibashiInstructionProgramDefinitions.HitboxProgram;

    /// <summary>Sound effect $61 in library two, queued when an eruption begins.</summary>
    internal const ushort EruptionSoundEffect = 0x0061;

    /// <summary>
    /// $A6:8DBB/$A6:8DE7, the 22 eruption Y offsets and collision half-heights.
    /// </summary>
    internal static HibashiActivityDefinition ActivityFrame(int index)
    {
        if ((uint)index >= 22)
            throw new ArgumentOutOfRangeException(nameof(index));
        return new((ushort)((index + 1) * 5), (ushort)(24 - Math.Max(0, index - 17) * 4));
    }
}
