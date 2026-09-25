namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical draw layouts selected by the reachable right- and left-facing
/// Draygon cannon PLM programs. Unused diagonal orientations are not claimed.
/// These full level words and signed row offsets remain gameplay definitions.
/// </summary>
internal static class DraygonCannonPlmDrawDefinitions
{
    /// <summary>Right shield frame A at $84:9FCD.</summary>
    internal const ushort RightShieldA = 0x9fcd;
    /// <summary>Right shield frame B at $84:9FDD.</summary>
    internal const ushort RightShieldB = 0x9fdd;
    /// <summary>Right damaged frame A at $84:A02D.</summary>
    internal const ushort RightDamagedA = 0xa02d;
    /// <summary>Right damaged frame B at $84:A03D.</summary>
    internal const ushort RightDamagedB = 0xa03d;
    /// <summary>Right damaged frame C at $84:A04D.</summary>
    internal const ushort RightDamagedC = 0xa04d;
    /// <summary>Right damaged frame D at $84:A05D.</summary>
    internal const ushort RightDamagedD = 0xa05d;
    /// <summary>Left shield frame A at $84:A0ED.</summary>
    internal const ushort LeftShieldA = 0xa0ed;
    /// <summary>Left shield frame B at $84:A101.</summary>
    internal const ushort LeftShieldB = 0xa101;
    /// <summary>Left damaged frame A at $84:A165.</summary>
    internal const ushort LeftDamagedA = 0xa165;
    /// <summary>Left damaged frame B at $84:A179.</summary>
    internal const ushort LeftDamagedB = 0xa179;
    /// <summary>Left damaged frame C at $84:A18D.</summary>
    internal const ushort LeftDamagedC = 0xa18d;
    /// <summary>Left damaged frame D at $84:A1A1.</summary>
    internal const ushort LeftDamagedD = 0xa1a1;

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    internal static string VisualId(ushort pointer) => pointer switch
    {
        RightShieldA => "right-shield-a",
        RightShieldB => "right-shield-b",
        RightDamagedA => "right-damaged-a",
        RightDamagedB => "right-damaged-b",
        RightDamagedC => "right-damaged-c",
        RightDamagedD => "right-damaged-d",
        LeftShieldA => "left-shield-a",
        LeftShieldB => "left-shield-b",
        LeftDamagedA => "left-damaged-a",
        LeftDamagedB => "left-damaged-b",
        LeftDamagedC => "left-damaged-c",
        LeftDamagedD => "left-damaged-d",
        _ => throw new InvalidDataException(
            $"Draygon cannon draw ${pointer:X4} has no visual ID."),
    };

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
        RoomPlmShotBlockDrawDefinitions.DrawList> Build() =>
        new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>
        {
            [RightShieldA] = Right(RightShieldA, 0xc514, 0x0513, 0xd534, 0x0533),
            [RightShieldB] = Right(RightShieldB, 0xc516, 0x0515, 0xd536, 0x0535),
            [RightDamagedA] = Right(RightDamagedA, 0xa580, 0x00ff, 0xa5a0, 0x00ff),
            [RightDamagedB] = Right(RightDamagedB, 0xa581, 0x00ff, 0xa5a1, 0x00ff),
            [RightDamagedC] = Right(RightDamagedC, 0xa582, 0x00ff, 0xa5a2, 0x00ff),
            [RightDamagedD] = Right(RightDamagedD, 0xa583, 0x00ff, 0xa5a3, 0x00ff),
            [LeftShieldA] = Left(LeftShieldA, 0xc114, 0x0113, 0x0133, 0xd134),
            [LeftShieldB] = Left(LeftShieldB, 0xc116, 0x0115, 0x0135, 0xd136),
            [LeftDamagedA] = Left(LeftDamagedA, 0xa180, 0x00ff, 0x00ff, 0xa1a0),
            [LeftDamagedB] = Left(LeftDamagedB, 0xa181, 0x00ff, 0x00ff, 0xa1a1),
            [LeftDamagedC] = Left(LeftDamagedC, 0xa182, 0x00ff, 0x00ff, 0xa1a2),
            [LeftDamagedD] = Left(LeftDamagedD, 0xa183, 0x00ff, 0x00ff, 0xa1a3),
        };

    private static RoomPlmShotBlockDrawDefinitions.DrawList Right(
        ushort pointer, ushort upperLeft, ushort upperRight,
        ushort lowerLeft, ushort lowerRight) => new(pointer,
        new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(2, new ushort[] { upperLeft, upperRight }, 0, 1),
            new(2, new ushort[] { lowerLeft, lowerRight }, 0, 0),
        });

    private static RoomPlmShotBlockDrawDefinitions.DrawList Left(
        ushort pointer, ushort upperRight, ushort upperLeft,
        ushort lowerLeft, ushort lowerRight) => new(pointer,
        new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(1, new ushort[] { upperRight }, -1, 0),
            new(1, new ushort[] { upperLeft }, -1, 1),
            new(2, new ushort[] { lowerLeft, lowerRight }, 0, 0),
        });
}
