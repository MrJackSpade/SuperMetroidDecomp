namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>VRAM layout and palette fields used by the bank-$82 pause-screen routines.</summary>
internal static class PauseMenuLayout
{
    /// <summary>BG1 tilemap base word used by the map and equipment pages.</summary>
    public const ushort Bg1TilemapWord = 0x3000;
    /// <summary>BG2 tilemap base word used by the pause-screen frame.</summary>
    public const ushort Bg2TilemapWord = 0x3800;
    /// <summary>Pause character/palette word used to blank the retained FX plane.</summary>
    public const ushort Bg3FxClearTile = 0x184e;
    /// <summary>Number of BG3 words below the four-row HUD.</summary>
    public const int Bg3FxClearWordCount = 0x0780;
    /// <summary>Byte offset of those rows in the bank-$B6 button tilemap.</summary>
    public const int ButtonRowsSourceOffset = 0x0240;
    /// <summary>Combined byte length of the two mutable button rows.</summary>
    public const int ButtonRowsByteCount = 0x0080;
    /// <summary>$7E:3400 button source expressed relative to the $7E:3000 native word indexes.</summary>
    public const int ButtonSourceWordOrigin = 0x0200;
    /// <summary>MAP, EXIT and SAMUS labels recolored by $82:A628-$A84C.</summary>
    public static IEnumerable<(int Word, int Count)> ButtonLabelSpans
    {
        get
        {
            for (int label = 0; label < 3; label++)
            for (int row = 0; row < 2; row++)
                yield return ButtonLabelSpan((PauseButtonLabel)label, row);
        }
    }

    /// <summary>Projects a named two-row button label into the 32-column BG2 tilemap.</summary>
    /// <remarks>$82:A633/A66F/A6AB select MAP/EXIT/SAMUS at columns5/12/22.
    /// Their lower halves are one32-word row below, as at A651/A68D/A6C9.
    /// The center EXIT label occupies four columns; the outside labels occupy five.</remarks>
    public static (int Word, int Count) ButtonLabelSpan(PauseButtonLabel label, int row)
    {
        int column = label switch
        {
            PauseButtonLabel.Map => 5,
            PauseButtonLabel.Exit => 12,
            PauseButtonLabel.Samus => 22,
            _ => throw new ArgumentOutOfRangeException(nameof(label)),
        };
        if ((uint)row >= 2) throw new ArgumentOutOfRangeException(nameof(row));
        return ((25 + row) * 32 + column, label == PauseButtonLabel.Exit ? 4 : 5);
    }
    /// <summary>OBSEL value installed by the pause-screen PPU setup.</summary>
    public const byte ObjectSelection = 0x01;
    /// <summary>$82:B9C8 selects OBJ palette 7 for the Samus position indicator, independently overridable by authored sprite parts.</summary>
    public const ushort MapMarkerPaletteBits = 0x0e00;
}
