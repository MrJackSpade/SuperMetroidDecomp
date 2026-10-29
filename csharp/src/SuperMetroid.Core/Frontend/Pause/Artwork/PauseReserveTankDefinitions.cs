namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Reserve-strip visual identities and import-only cartridge bindings.</summary>
public static class PauseReserveTankDefinitions
{
    /// <summary>Supported schema version for the editable reserve-strip anchors, palette, and visual frames.</summary>
    public const int Version = 1;
    /// <summary>JSON asset filename for the pause equipment screen's reserve-tank strip presentation.</summary>
    public const string FileName = "pause-reserve-tanks.json";
    /// <summary>$82:C1D6 supplies six origins, including the trailing cap position.</summary>
    public const int AnchorCount = 6;
    /// <summary>The sprite palette every reserve-gauge part is drawn with.</summary>
    public const int Palette = 3;
    /// <summary>$82:C1D6: screen X of the first tank's anchor.</summary>
    internal const int FirstAnchorX = 0x18;
    /// <summary>$82:C1D6..C1E2: horizontal distance between neighbouring tank anchors, one eight-pixel column.</summary>
    internal const int AnchorStride = 8;
    /// <summary>$82:C1D6..C1E2: screen Y shared by every anchor, Y96 minus the draw bias.</summary>
    internal const int AnchorY = 0x5f;
    /// <summary>$82:B3D9: the two identical eight-entry partial-fill tables together hold sixteen entries.</summary>
    internal const int PartialMapEntries = 16;

    /// <summary>$82:B305 full tank; $82:B396 trailing cap; $82:B37D empty tank and $82:B3D9 seven fill levels, as ten stable
    /// frame-name/native-spritemap pairs in full, end-cap, empty, then Fill1..Fill7 order.</summary>
    internal static readonly PauseReserveTankFrame[] Frames =
    [
        new("Full", PauseReserveTankVisual.Full),
        new("EndCap", PauseReserveTankVisual.EndCap),
        new("Empty", PauseReserveTankVisual.Empty),
        new("Fill1", PauseReserveTankVisual.Fill1),
        new("Fill2", PauseReserveTankVisual.Fill2),
        new("Fill3", PauseReserveTankVisual.Fill3),
        new("Fill4", PauseReserveTankVisual.Fill4),
        new("Fill5", PauseReserveTankVisual.Fill5),
        new("Fill6", PauseReserveTankVisual.Fill6),
        new("Fill7", PauseReserveTankVisual.Fill7),
    ];

    /// <summary>The partial-tank spritemap identity for each fill-step count, empty (zero steps) through seven steps.</summary>
    internal static readonly PauseReserveTankVisual[] FillLevels =
    [
        PauseReserveTankVisual.Empty,
        PauseReserveTankVisual.Fill1,
        PauseReserveTankVisual.Fill2,
        PauseReserveTankVisual.Fill3,
        PauseReserveTankVisual.Fill4,
        PauseReserveTankVisual.Fill5,
        PauseReserveTankVisual.Fill6,
        PauseReserveTankVisual.Fill7,
    ];
}
