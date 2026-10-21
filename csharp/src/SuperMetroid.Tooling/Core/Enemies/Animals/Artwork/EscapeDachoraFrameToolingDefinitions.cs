
namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>Development-tool members of <see cref="EscapeDachoraFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class EscapeDachoraFrameToolingDefinitions
{
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EscapeDachoraFrame frame) => frame.Pose switch
    {
        0 => 0xeb1b,
        1 => 0xeb4a,
        2 => 0xeb79,
        3 => 0xebb2,
        4 => 0xebe1,
        5 => 0xec10,
        6 => 0xec49,
        7 => 0xec78,
        8 => 0xeca7,
        9 => 0xece0,
        10 => 0xed0f,
        11 => 0xed3e,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined escape Dachora pose."),
    };
}
