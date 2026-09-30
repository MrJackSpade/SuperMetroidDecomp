namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical four-block level writes for the four opening frames of each blue-door cap.
/// The bank-$84 instruction programs still own sound, frame timing, and deletion.
/// </summary>
internal static class BlueDoorPlmDrawDefinitions
{
    /// <summary>Left-facing blue-cap frame zero at $84:A9B3.</summary>
    private const ushort LeftFrame0 = 0xa9b3;
    /// <summary>Right-facing blue-cap frame zero at $84:A9EF.</summary>
    private const ushort RightFrame0 = 0xa9ef;
    /// <summary>Up-facing blue-cap frame zero at $84:AA2B.</summary>
    private const ushort UpFrame0 = 0xaa2b;
    /// <summary>Down-facing blue-cap frame zero at $84:AA67.</summary>
    private const ushort DownFrame0 = 0xaa67;
    /// <summary>Byte length of each native four-block draw list including its terminator.</summary>
    internal const int DrawListBytes = 12;

    private static readonly Dictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;
    /// <summary>Opening frames also provide closed-cap appearance; the physical first-word collision differs.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> Editable =>
        Lists.Values.Where(draw => VisualSource(draw.Pointer) == draw.Pointer);

    internal static ushort VisualSource(ushort pointer) => pointer switch
    {
        LeftFrame0 - DrawListBytes => LeftFrame0,
        RightFrame0 - DrawListBytes => RightFrame0,
        UpFrame0 - DrawListBytes => UpFrame0,
        DownFrame0 - DrawListBytes => DownFrame0,
        _ => pointer,
    };

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    internal static string VisualId(ushort pointer)
    {
        pointer = VisualSource(pointer);
        foreach ((ushort first, string direction) in new[]
                 {
                     (LeftFrame0, "left"), (RightFrame0, "right"),
                     (UpFrame0, "up"), (DownFrame0, "down"),
                 })
        {
            int offset = pointer - first;
            if (offset >= 0 && offset < 4 * DrawListBytes &&
                offset % DrawListBytes == 0)
                return $"{direction}-frame-{offset / DrawListBytes}";
        }
        throw new InvalidDataException($"Blue-door draw ${pointer:X4} has no visual ID.");
    }

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in Editable)
        {
            if (string.Equals(id, VisualId(candidate.Pointer), StringComparison.Ordinal))
            {
                list = candidate;
                return true;
            }
        }
        list = default;
        return false;
    }

    private static Dictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build()
    {
        var lists = new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>(20);
        AddFourFrames(lists, LeftFrame0, 0x8004,
            [0xc00c, 0xd02c, 0xd82c, 0xd80c],
            [0x800d, 0x802d, 0x882d, 0x880d],
            [0x800e, 0x802e, 0x882e, 0x880e],
            [0x800f, 0x002f, 0x082f, 0x880f]);
        AddFourFrames(lists, RightFrame0, 0x8004,
            [0xc40c, 0xd42c, 0xdc2c, 0xdc0c],
            [0x840d, 0x842d, 0x8c2d, 0x8c0d],
            [0x840e, 0x842e, 0x8c2e, 0x8c0e],
            [0x840f, 0x042f, 0x0c2f, 0x8c0f]);
        AddFourFrames(lists, UpFrame0, 0x0004,
            [0xc41d, 0x541c, 0x501c, 0x501d],
            [0x843d, 0x843c, 0x803c, 0x803d],
            [0x841f, 0x841e, 0x801e, 0x801f],
            [0x843f, 0x843e, 0x803e, 0x803f]);
        AddFourFrames(lists, DownFrame0, 0x0004,
            [0xcc1d, 0x5c1c, 0x581c, 0x581d],
            [0x8c3d, 0x8c3c, 0x883c, 0x883d],
            [0x8c1f, 0x8c1e, 0x881e, 0x881f],
            [0x8c3f, 0x8c3e, 0x883e, 0x883f]);
        // Each preceding native closed-cap list draws the same four visual words
        // as frame zero, but clears shootable type C on its first solid block.
        // Compile all four orientations, including the two eye-door conversion users.
        foreach (ushort first in new ushort[] { LeftFrame0, RightFrame0, UpFrame0, DownFrame0 })
        {
            RoomPlmShotBlockDrawDefinitions.Run opening = lists[first].Runs.Span[0];
            ushort[] words = opening.LevelWords.ToArray();
            words[0] = new RoomLevelWord(words[0]).WithCollisionType(RoomCollisionType.SolidBlock).Raw;
            ushort pointer = checked((ushort)(first - DrawListBytes));
            lists.Add(pointer, new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[]
                { new(opening.DirectionAndCount, words, 0, 0) }));
        }
        return lists;
    }

    private static void AddFourFrames(
        Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList> lists,
        ushort firstPointer,
        ushort directionAndCount,
        params ushort[][] frames)
    {
        if (frames.Length != 4)
            throw new InvalidDataException("A blue-door cap needs exactly four draw frames.");
        for (int frame = 0; frame < frames.Length; frame++)
        {
            if (frames[frame].Length != 4)
                throw new InvalidDataException("A blue-door draw frame needs four blocks.");
            ushort pointer = checked((ushort)(firstPointer + frame * DrawListBytes));
            if (!lists.TryAdd(pointer, new(pointer,
                new RoomPlmShotBlockDrawDefinitions.Run[]
                {
                    new(directionAndCount, frames[frame], 0, 0),
                })))
                throw new InvalidDataException($"Duplicate blue-door draw list ${pointer:X4}.");
        }
    }
}
