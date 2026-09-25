namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical four-block draw lists for ordinary yellow, green, and red door caps.
/// The resident PLM programs retain their hit filters, counters, timing, sound,
/// door-bit persistence, and eventual blue-cap conversion.
/// </summary>
internal static class ColoredDoorPlmDrawDefinitions
{
    /// <summary>Yellow left-facing door, first draw at $84:A767.</summary>
    private const ushort YellowLeft = 0xa767;
    /// <summary>Yellow right-facing door, first draw at $84:A797.</summary>
    private const ushort YellowRight = 0xa797;
    /// <summary>Yellow upward-facing door, first draw at $84:A7C7.</summary>
    private const ushort YellowUp = 0xa7c7;
    /// <summary>Yellow downward-facing door, first draw at $84:A7F7.</summary>
    private const ushort YellowDown = 0xa7f7;
    /// <summary>Green left-facing door, first draw at $84:A827.</summary>
    private const ushort GreenLeft = 0xa827;
    /// <summary>Green right-facing door, first draw at $84:A857.</summary>
    private const ushort GreenRight = 0xa857;
    /// <summary>Green upward-facing door, first draw at $84:A887.</summary>
    private const ushort GreenUp = 0xa887;
    /// <summary>Green downward-facing door, first draw at $84:A8B7.</summary>
    private const ushort GreenDown = 0xa8b7;
    /// <summary>Red left-facing door, first draw at $84:A8E7.</summary>
    private const ushort RedLeft = 0xa8e7;
    /// <summary>Red right-facing door, first draw at $84:A917.</summary>
    private const ushort RedRight = 0xa917;
    /// <summary>Red upward-facing door, first draw at $84:A947.</summary>
    private const ushort RedUp = 0xa947;
    /// <summary>Red downward-facing door, first draw at $84:A977.</summary>
    private const ushort RedDown = 0xa977;

    internal const int DrawListBytes = 12;

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    internal static string VisualId(ushort pointer)
    {
        foreach ((ushort first, string name) in new[]
                 {
                     (YellowLeft, "yellow-left"), (YellowRight, "yellow-right"),
                     (YellowUp, "yellow-up"), (YellowDown, "yellow-down"),
                     (GreenLeft, "green-left"), (GreenRight, "green-right"),
                     (GreenUp, "green-up"), (GreenDown, "green-down"),
                     (RedLeft, "red-left"), (RedRight, "red-right"),
                     (RedUp, "red-up"), (RedDown, "red-down"),
                 })
        {
            int offset = pointer - first;
            if (offset >= 0 && offset < 4 * DrawListBytes &&
                offset % DrawListBytes == 0)
                return $"{name}-frame-{offset / DrawListBytes}";
        }
        throw new InvalidDataException($"Colored-door draw ${pointer:X4} has no visual ID.");
    }

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in Lists.Values)
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

    private static IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build()
    {
        var lists = new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>(48);
        AddFourFrames(lists, YellowLeft, 0x8004,
            [0xc000, 0xd020, 0xd820, 0xd800],
            [0x8001, 0x8021, 0x8821, 0x8801],
            [0x8002, 0x8022, 0x8822, 0x8802],
            [0x8003, 0x0023, 0x0823, 0x8803]);
        AddFourFrames(lists, YellowRight, 0x8004,
            [0xc400, 0xd420, 0xdc20, 0xdc00],
            [0x8401, 0x8421, 0x8c21, 0x8c01],
            [0x8402, 0x8422, 0x8c22, 0x8c02],
            [0x8403, 0x8423, 0x8c23, 0x8c03]);
        AddFourFrames(lists, YellowUp, 0x0004,
            [0xc411, 0x5410, 0x5010, 0x5011],
            [0x8431, 0x8430, 0x8030, 0x8031],
            [0x8413, 0x8412, 0x8012, 0x8013],
            [0x8433, 0x8432, 0x8032, 0x8033]);
        AddFourFrames(lists, YellowDown, 0x0004,
            [0xcc11, 0x5c10, 0x5810, 0x5811],
            [0x8c31, 0x8c30, 0x8830, 0x8831],
            [0x8c13, 0x8c12, 0x8812, 0x8813],
            [0x8c33, 0x0c32, 0x0832, 0x8833]);
        AddFourFrames(lists, GreenLeft, 0x8004,
            [0xc004, 0xd024, 0xd824, 0xd804],
            [0x8005, 0x8025, 0x8825, 0x8805],
            [0x8006, 0x8026, 0x8826, 0x8806],
            [0x8007, 0x0027, 0x0827, 0x8807]);
        AddFourFrames(lists, GreenRight, 0x8004,
            [0xc404, 0xd424, 0xdc24, 0xdc04],
            [0x8405, 0x8425, 0x8c25, 0x8c05],
            [0x8406, 0x8426, 0x8c26, 0x8c06],
            [0x8407, 0x0427, 0x0c27, 0x8c07]);
        AddFourFrames(lists, GreenUp, 0x0004,
            [0xc415, 0x5414, 0x5014, 0x5015],
            [0x8435, 0x8434, 0x8034, 0x8035],
            [0x8417, 0x8416, 0x8016, 0x8017],
            [0x8437, 0x8436, 0x8036, 0x8037]);
        AddFourFrames(lists, GreenDown, 0x0004,
            [0xcc15, 0x5c14, 0x5814, 0x5815],
            [0x8c35, 0x8c34, 0x8834, 0x8835],
            [0x8c17, 0x8c16, 0x8816, 0x8817],
            [0x8c37, 0x8c36, 0x8836, 0x8837]);
        AddFourFrames(lists, RedLeft, 0x8004,
            [0xc008, 0xd028, 0xd828, 0xd808],
            [0x8009, 0x8029, 0x8829, 0x8809],
            [0x800a, 0x802a, 0x882a, 0x880a],
            [0x800b, 0x002b, 0x082b, 0x880b]);
        AddFourFrames(lists, RedRight, 0x8004,
            [0xc408, 0xd428, 0xdc28, 0xdc08],
            [0x8409, 0x8429, 0x8c29, 0x8c09],
            [0x840a, 0x842a, 0x8c2a, 0x8c0a],
            [0x840b, 0x042b, 0x0c2b, 0x8c0b]);
        AddFourFrames(lists, RedUp, 0x0004,
            [0xc419, 0x5418, 0x5018, 0x5019],
            [0x8439, 0x8438, 0x8038, 0x8039],
            [0x841b, 0x841a, 0x801a, 0x801b],
            [0x843b, 0x843a, 0x803a, 0x803b]);
        AddFourFrames(lists, RedDown, 0x0004,
            [0xcc19, 0x5c18, 0x5818, 0x5819],
            [0x8c39, 0x8c38, 0x8838, 0x8839],
            [0x8c1b, 0x8c1a, 0x881a, 0x881b],
            [0x8c3b, 0x8c3a, 0x883a, 0x883b]);
        return lists;
    }

    private static void AddFourFrames(
        Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList> lists,
        ushort firstPointer,
        ushort directionAndCount,
        params ushort[][] frames)
    {
        if (frames.Length != 4)
            throw new InvalidDataException("A colored cap needs exactly four draw frames.");
        for (int frame = 0; frame < frames.Length; frame++)
        {
            if (frames[frame].Length != 4)
                throw new InvalidDataException("A colored-cap frame needs four blocks.");
            ushort pointer = checked((ushort)(firstPointer + frame * DrawListBytes));
            if (!lists.TryAdd(pointer, new(pointer,
                    new RoomPlmShotBlockDrawDefinitions.Run[]
                    {
                        new(directionAndCount, frames[frame], 0, 0),
                    })))
                throw new InvalidDataException(
                    $"Duplicate colored-door draw list ${pointer:X4}.");
        }
    }
}
