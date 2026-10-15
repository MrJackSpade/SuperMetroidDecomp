namespace SuperMetroid.Core.Rooms.Plms.Artwork;

/// <summary>
/// The presentation boundary of the save-station electricity's programs (#1296): the installed artwork key of each
/// <see cref="SaveStationElectricityFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class SaveStationElectricityFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 8;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(SaveStationElectricityFrame frame) => frame.Pose switch
    {
        0 => 0xe689,
        1 => 0xe68d,
        2 => 0xe691,
        3 => 0xe695,
        4 => 0xe699,
        5 => 0xe69d,
        6 => 0xe6a1,
        7 => 0xe6a5,
        _ => throw new InvalidOperationException($"SaveStationElectricity pose {frame.Pose} is undefined."),
    };
}
