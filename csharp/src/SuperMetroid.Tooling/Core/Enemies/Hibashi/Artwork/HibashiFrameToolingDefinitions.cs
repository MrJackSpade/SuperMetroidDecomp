namespace SuperMetroid.Core.Enemies.Hibashi.Artwork;

/// <summary>Development-tool members of <see cref="HibashiFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class HibashiFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(HibashiFrame frame) => frame.Pose switch
    {
        0 => 0x9082,
        1 => 0x9089,
        2 => 0x909f,
        3 => 0x90ba,
        4 => 0x90da,
        5 => 0x90ff,
        6 => 0x9124,
        7 => 0x9149,
        8 => 0x916e,
        9 => 0x9198,
        10 => 0x91c2,
        11 => 0x91f1,
        12 => 0x9234,
        13 => 0x927c,
        14 => 0x92c4,
        15 => 0x9302,
        16 => 0x934a,
        17 => 0x938d,
        18 => 0x93cb,
        19 => 0x9404,
        20 => 0x942e,
        21 => 0x944e,
        22 => 0x9469,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Hibashi pose."),
    };
}
