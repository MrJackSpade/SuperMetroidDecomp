namespace SuperMetroid.Core.Enemies.KiHunters.Artwork;

/// <summary>Development-tool members of <see cref="KiHunterFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class KiHunterFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(KiHunterFrame frame) => frame.Pose switch
    {
        0 => 0xeb2e,
        1 => 0xeb58,
        2 => 0xeb82,
        3 => 0xebac,
        4 => 0xebc2,
        5 => 0xebce,
        6 => 0xebf7,
        7 => 0xec21,
        8 => 0xec4b,
        9 => 0xec75,
        10 => 0xec8b,
        11 => 0xec97,
        12 => 0xeca8,
        13 => 0xecc0,
        14 => 0xecea,
        15 => 0xed14,
        16 => 0xed3e,
        17 => 0xed68,
        18 => 0xed92,
        19 => 0xedbc,
        20 => 0xede6,
        21 => 0xee10,
        22 => 0xee3a,
        23 => 0xee64,
        24 => 0xee8e,
        25 => 0xeeb8,
        26 => 0xeee2,
        27 => 0xef0c,
        28 => 0xef36,
        29 => 0xef60,
        30 => 0xef8a,
        31 => 0xefb4,
        32 => 0xefde,
        33 => 0xf00d,
        34 => 0xf03c,
        35 => 0xf06b,
        36 => 0xf09a,
        37 => 0xf0c4,
        38 => 0xf0f3,
        39 => 0xf122,
        40 => 0xf151,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ki-Hunter pose."),
    };
}
