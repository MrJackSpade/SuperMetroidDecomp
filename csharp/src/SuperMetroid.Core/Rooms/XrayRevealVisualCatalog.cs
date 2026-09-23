namespace SuperMetroid.Core.Rooms;

/// <summary>Replaceable metatile references for one cartridge-defined X-ray reveal.</summary>
public readonly record struct XrayRevealVisualWords(
    ushort TopLeft,
    ushort TopRight,
    ushort BottomLeft,
    ushort BottomRight);

/// <summary>
/// Installed visual choices for X-ray blocks. The cartridge's collision/BTS lookup,
/// copy dimensions, extension traversal, and Brinstar-only condition remain compiled.
/// </summary>
public sealed class XrayRevealVisualCatalog
{
    private readonly XrayRevealVisualWords?[] words = new XrayRevealVisualWords?[16 * 256];

    /// <summary>Creates a complete visual replacement for every drawable native rule.</summary>
    public XrayRevealVisualCatalog(IEnumerable<(RoomCollisionType Type, byte Bts,
        XrayRevealVisualWords Visual)> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        foreach ((RoomCollisionType type, byte bts, XrayRevealVisualWords visual) in entries)
        {
            XrayRevealDefinition? native = XrayRevealTable.Find(type, bts);
            if (native is not { } definition || !IsDrawable(definition.Command))
                throw new InvalidDataException(
                    $"X-ray visual {type}/BTS ${bts:X2} has no drawable cartridge rule.");
            int index = ((int)type << 8) | bts;
            if (words[index] is not null)
                throw new InvalidDataException(
                    $"X-ray visual {type}/BTS ${bts:X2} appears more than once.");
            words[index] = visual;
        }

        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            bool drawable = XrayRevealTable.Find(type, unchecked((byte)bts)) is { } definition &&
                IsDrawable(definition.Command);
            if (drawable != (words[((int)type << 8) | bts] is not null))
                throw new InvalidDataException(
                    $"X-ray visual catalog does not cover cartridge rule {type}/BTS ${bts:X2}.");
        }
    }

    /// <summary>Substitutes visual operands without altering the compiled native command.</summary>
    public XrayRevealDefinition Apply(RoomCollisionType type, byte bts,
        XrayRevealDefinition native)
    {
        if (!IsDrawable(native.Command)) return native;
        XrayRevealVisualWords visual = words[((int)type << 8) | bts] ??
            throw new InvalidDataException($"Missing X-ray visual {type}/BTS ${bts:X2}.");
        return native with
        {
            TopLeft = visual.TopLeft,
            TopRight = visual.TopRight,
            BottomLeft = visual.BottomLeft,
            BottomRight = visual.BottomRight,
        };
    }

    /// <summary>Whether this compiled command carries authored visual metatile operands.</summary>
    public static bool IsDrawable(ushort command) => command is
        XrayRevealCodePointers.CopyOne or XrayRevealCodePointers.CopyWide or
        XrayRevealCodePointers.CopyTall or XrayRevealCodePointers.CopySquare or
        XrayRevealCodePointers.CopyBrinstar;
}
