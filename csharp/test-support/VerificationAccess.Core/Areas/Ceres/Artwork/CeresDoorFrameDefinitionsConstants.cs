namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Cartridge values and helpers of <see cref="CeresDoorFrameDefinitions"/> that only verification reads.</summary>
internal static class CeresDoorFrameDefinitionsConstants
{
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(CeresDoorFrame frame) => frame.Pose switch
    {
        0 => 0xf921,
        1 => 0xf95f,
        2 => 0xf989,
        3 => 0xf9b3,
        4 => 0xf9d3,
        5 => 0xf9f3,
        6 => 0xfa13,
        7 => 0xfa3d,
        8 => 0xfa67,
        9 => 0xfa87,
        10 => 0xfaa7,
        11 => 0xface,
        12 => 0xfb2f,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ceres door pose."),
    };
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The Ridley-room door overlay $A6:A329 that Ridley's scene draws while it holds the door.</summary>
    internal const ushort RidleyPrivateOverlaySpritemap = 0xa329;
}
