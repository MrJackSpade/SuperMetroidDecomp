namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Four native two-by-two physical ceiling draws. Each list has two
/// horizontal runs; editable visible block selections are a separate asset.
/// </summary>
internal static class SporeSpawnCeilingPlmDrawDefinitions
{
    /// <summary><c>$84:9413</c>: clear the ceiling after defeat or crumble.</summary>
    internal const ushort ClearPointer = 0x9413;
    /// <summary><c>$84:9423</c>: first crumble appearance.</summary>
    internal const ushort CrumbleFirstPointer = 0x9423;
    /// <summary><c>$84:9433</c>: second crumble appearance.</summary>
    internal const ushort CrumbleSecondPointer = 0x9433;
    /// <summary><c>$84:9443</c>: third crumble appearance.</summary>
    internal const ushort CrumbleThirdPointer = 0x9443;
    /// <summary><c>$84:9453</c>: first byte of the following Mother Brain draw region.</summary>
    internal const ushort EndExclusive = 0x9453;

    private static readonly RoomPlmShotBlockDrawDefinitions.DrawList[] Lists =
    [
        Square(ClearPointer, 0x00ff),
        Square(CrumbleFirstPointer, 0x0053),
        Square(CrumbleSecondPointer, 0x0054),
        Square(CrumbleThirdPointer, 0x0055),
    ];

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists;

    internal static ushort CrumbleFramePointer(int frame) => frame switch
    {
        0 => CrumbleFirstPointer,
        1 => CrumbleSecondPointer,
        2 => CrumbleThirdPointer,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in Lists)
        {
            if (candidate.Pointer != pointer)
                continue;
            list = candidate;
            return true;
        }
        list = default;
        return false;
    }

    private static RoomPlmShotBlockDrawDefinitions.DrawList Square(
        ushort pointer, ushort physicalWord) => new(pointer,
        new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new RoomPlmShotBlockDrawDefinitions.Run(
                2, new ushort[] { physicalWord, physicalWord }, 0, 1),
            new RoomPlmShotBlockDrawDefinitions.Run(
                2, new ushort[] { physicalWord, physicalWord }, 0, 0),
        });
}
