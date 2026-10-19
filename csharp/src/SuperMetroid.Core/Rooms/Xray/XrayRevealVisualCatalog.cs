using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Rooms.Xray;

/// <summary>
/// Installed visual choices for X-ray blocks. The cartridge's collision/BTS lookup,
/// copy dimensions, extension traversal, and Brinstar-only condition remain compiled.
/// </summary>
public sealed class XrayRevealVisualCatalog
{
    /// <summary>Installed visual operands indexed by collision nibble and BTS byte.</summary>
    /// <remarks>
    /// Issue #1037: the exact index is (type &lt;&lt; 8) | bts over 16 native
    /// collision nibbles and 256 unsigned BTS values. Of 4096 possible
    /// slots, the pinned NTSC J/U v1.0 ROM has 305 drawable pairs; the
    /// constructor requires exactly those pairs and rejects duplicates.
    /// Stock operands come from bank-$91 reveal command records and all
    /// 305 installed values match the independent cartridge oracle.
    /// The visual operands can also be replaced through reveals.json while
    /// the native command and copy dimensions remain fixed. Therefore no
    /// single ROM-derived formula can reproduce every valid installation:
    /// retain this bounded indexed store for the selected visual data.
    /// </remarks>
    private readonly XrayRevealVisualWords?[] words = new XrayRevealVisualWords?[16 * 256];

    /// <summary>Creates a complete visual replacement for every drawable native rule.</summary>
    /// <param name="entries">Exactly one set of visual operands for each drawable collision-nibble/BTS pair; native command selection and copy geometry remain fixed.</param>
    /// <param name="overlays">Installed item/special-room visuals, or null for a noninstalled fixture.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry has no drawable rule, duplicates another pair, or leaves native drawable coverage incomplete.</exception>
    public XrayRevealVisualCatalog(IEnumerable<(RoomCollisionType Type, byte Bts,
        XrayRevealVisualWords Visual)> entries, XrayOverlayVisualCatalog? overlays = null)
    {
        Ensure.NotNull(entries);
        Overlays = overlays;
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

    /// <summary>Installed item and special-room visuals; null only in noninstalled fixtures.</summary>
    public XrayOverlayVisualCatalog? Overlays { get; }

    /// <summary>Identity of every selected drawable rule and its installed overlays, excluding command mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(XrayRevealVisualCatalog), content =>
    {
        for (int index = 0; index < words.Length; index++)
            if (words[index] is { } visual)
            {
                content.Append("type and bts", index);
                content.AppendWords("visual operands",
                    [visual.TopLeft, visual.TopRight, visual.BottomLeft, visual.BottomRight]);
            }
        content.Append("has overlays", Overlays is null ? 0 : 1);
        if (Overlays is not null)
            content.Append("overlays", Convert.FromHexString(Overlays.ContentIdentity));
    });

    /// <summary>
    /// Resolves the compiled command and its installed visual operands from the same
    /// collision/BTS identity. Callers cannot pair a drawable command with another
    /// block's absent artwork; unowned pairs retain the native no-reveal result.
    /// </summary>
    public XrayRevealDefinition? Apply(RoomCollisionType type, int bts)
    {
        if (XrayRevealTable.Find(type, bts) is not { } native) return null;
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
    /// <remarks>
    /// Issue #1036: the pinned NTSC J/U v1.0 ROM has seven reachable
    /// revealed-block commands. $91:CF36, CF3E, CF4E, CF62, and CF6F
    /// copy one, Brinstar-only one, wide, tall, and square operand tiles;
    /// $91:CE79 and CEBB traverse vertical and horizontal extensions
    /// instead. The exact bounded classification is membership in those
    /// five copy identities, with false for every other ushort. The
    /// independent 16-by-256 native reveal oracle finds all seven commands;
    /// its drawable BTS groups total 305, matching the installed visual
    /// catalog's exhaustive coverage assertion. Named membership is
    /// clearer than a pointer interval, since the two extension routines
    /// precede the copy commands but are not drawable.
    /// </remarks>
    public static bool IsDrawable(XrayRevealCommand command) => command is
        XrayRevealCommand.CopyOne or XrayRevealCommand.CopyWide or
        XrayRevealCommand.CopyTall or XrayRevealCommand.CopySquare or
        XrayRevealCommand.CopyBrinstar;
}
