namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical bank-$84 draw payloads for both mirrored, three-component eye doors.
/// The eye-door PLM instruction streams still control timing, attacks, hits,
/// persistence, and the eventual conversion to an ordinary blue door.
/// </summary>
internal static class EyeDoorPlmDrawDefinitions
{
    /// <summary>Left-facing eye animation begins at $84:9C03.</summary>
    private const ushort LeftEyeFirst = 0x9c03;
    /// <summary>Left-facing middle component begins at $84:9C2B.</summary>
    private const ushort LeftMiddleFirst = 0x9c2b;
    /// <summary>Left-facing bottom component begins at $84:9C3D.</summary>
    private const ushort LeftBottomFirst = 0x9c3d;
    /// <summary>Left-facing eye's four-block clearing frame is $84:9C4F.</summary>
    private const ushort LeftEyeClear = 0x9c4f;
    /// <summary>Right-facing eye animation begins at $84:9C5B.</summary>
    private const ushort RightEyeFirst = 0x9c5b;
    /// <summary>Right-facing middle component begins at $84:9C83.</summary>
    private const ushort RightMiddleFirst = 0x9c83;
    /// <summary>Right-facing bottom component begins at $84:9C95.</summary>
    private const ushort RightBottomFirst = 0x9c95;

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    private static IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build()
    {
        var lists = new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>(23);
        Add(lists, LeftEyeFirst + 0, 0x8002, [0x84cc, 0x8ccc]);
        Add(lists, LeftEyeFirst + 8, 0x8002, [0x84cb, 0x8ccb]);
        Add(lists, LeftEyeFirst + 16, 0x8002, [0xc4ca, 0xdcca]);
        Add(lists, LeftEyeFirst + 24, 0x8002, [0x84cd, 0x8ccd]);
        Add(lists, LeftEyeFirst + 32, 0x8002, [0x84ca, 0x8cca]);
        Add(lists, LeftMiddleFirst + 0, 0x0001, [0xa4aa]);
        Add(lists, LeftMiddleFirst + 6, 0x0001, [0xa4ab]);
        Add(lists, LeftMiddleFirst + 12, 0x0001, [0xa4ac]);
        Add(lists, LeftBottomFirst + 0, 0x0001, [0xacaa]);
        Add(lists, LeftBottomFirst + 6, 0x0001, [0xacab]);
        Add(lists, LeftBottomFirst + 12, 0x0001, [0xacac]);
        Add(lists, LeftEyeClear, 0x8004, [0x80aa, 0x80cc, 0x88cc, 0x88aa]);
        Add(lists, RightEyeFirst + 0, 0x8002, [0x80cc, 0x88cc]);
        Add(lists, RightEyeFirst + 8, 0x8002, [0x80cb, 0x88cb]);
        Add(lists, RightEyeFirst + 16, 0x8002, [0xc0ca, 0xd8ca]);
        Add(lists, RightEyeFirst + 24, 0x8002, [0x80cd, 0x88cd]);
        Add(lists, RightEyeFirst + 32, 0x8002, [0x80ca, 0x88ca]);
        Add(lists, RightMiddleFirst + 0, 0x0001, [0xa0aa]);
        Add(lists, RightMiddleFirst + 6, 0x0001, [0xa0ab]);
        Add(lists, RightMiddleFirst + 12, 0x0001, [0xa0ac]);
        Add(lists, RightBottomFirst + 0, 0x0001, [0xa8aa]);
        Add(lists, RightBottomFirst + 6, 0x0001, [0xa8ab]);
        Add(lists, RightBottomFirst + 12, 0x0001, [0xa8ac]);
        return lists;
    }

    private static void Add(
        Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList> lists,
        int pointer, ushort directionAndCount, ushort[] words)
    {
        if (words.Length != (directionAndCount & 0x7fff))
            throw new InvalidDataException($"Eye-door draw ${pointer:X4} count disagrees with its payload.");
        if (!lists.TryAdd(checked((ushort)pointer), new(checked((ushort)pointer),
                new RoomPlmShotBlockDrawDefinitions.Run[]
                {
                    new(directionAndCount, words, 0, 0),
                })))
            throw new InvalidDataException($"Duplicate eye-door draw ${pointer:X4}.");
    }
}
