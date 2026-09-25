namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The four horizontal crumble frames and six-row clear draw selected by the
/// Tourian access-floor PLMs. These words are physical level mutations;
/// replaceable block appearance is a separate concern.
/// </summary>
internal static class TourianAccessPlmDrawDefinitions
{
    /// <summary><c>$84:9297</c>: clear one row after its crumble frames.</summary>
    internal const ushort EmptyRowPointer = 0x9297;
    /// <summary><c>$84:92A3</c>: first visible crumble row.</summary>
    internal const ushort CrumbleFirstPointer = 0x92a3;
    /// <summary><c>$84:92AF</c>: second visible crumble row.</summary>
    internal const ushort CrumbleSecondPointer = 0x92af;
    /// <summary><c>$84:92BB</c>: third visible crumble row.</summary>
    internal const ushort CrumbleThirdPointer = 0x92bb;
    /// <summary><c>$84:92C7</c>: clear all six rows at once.</summary>
    internal const ushort ClearPointer = 0x92c7;

    private static readonly RoomPlmShotBlockDrawDefinitions.DrawList[] Lists =
    [
        Row(EmptyRowPointer, 0x00ff),
        Row(CrumbleFirstPointer, 0x0053),
        Row(CrumbleSecondPointer, 0x0054),
        Row(CrumbleThirdPointer, 0x0055),
        new(ClearPointer,
            Enumerable.Range(0, 6).Select(row =>
                new RoomPlmShotBlockDrawDefinitions.Run(
                    4, new ushort[] { 0x00ff, 0x00ff, 0x00ff, 0x00ff },
                    0, checked((sbyte)(row == 5 ? 0 : row + 1))))
                .ToArray()),
    ];

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists;

    internal static ushort CrumbleFramePointer(int frame) => frame switch
    {
        0 => CrumbleFirstPointer,
        1 => CrumbleSecondPointer,
        2 => CrumbleThirdPointer,
        3 => EmptyRowPointer,
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

    private static RoomPlmShotBlockDrawDefinitions.DrawList Row(
        ushort pointer, ushort levelWord) => new(pointer,
        new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(4, new ushort[] { levelWord, levelWord, levelWord, levelWord },
                0, 0),
        });
}
