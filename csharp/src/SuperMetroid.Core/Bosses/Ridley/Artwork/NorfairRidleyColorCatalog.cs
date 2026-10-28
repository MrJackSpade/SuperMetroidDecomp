using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Editable initial OBJ colors and fifteen rows of fourteen arena-reveal BG colors for Lower Norfair Ridley.</summary>
public sealed class NorfairRidleyColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => NorfairRidleyColors.ContentIdentity(this);

    /// <summary>Compiled initial OBJ colors, with shared paint relationships retained by the selector.</summary>
    internal readonly NorfairRidleyInitialPaint initial;
    /// <summary>Fifteen ordered arena background rows, excluding the runtime terminator.</summary>
    internal readonly NorfairRidleyRevealPaint reveal;

    /// <summary>Copies validated initial and reveal RGB5 colors into independent paint selectors.</summary>
    internal NorfairRidleyColorCatalog(RgbColor[] initial, RgbColor[][] reveal) =>
        (this.initial, this.reveal) = (new NorfairRidleyInitialPaint(initial), new NorfairRidleyRevealPaint(reveal));

    /// <summary>Installs the 32 initial RGB5 colors in CGRAM entries 160..191 (OBJ palettes 2 and 3), matching the native copy from <c>$A6:E1CF</c>; subsequent palette clearing is the caller's responsibility.</summary>
    /// <param name="colours">The colours to update.</param>
    public void ApplyInitial(GameplayColours colours) => NorfairRidleyColors.ApplyInitial(initial, colours);

    /// <summary>Copies one arena-reveal row to CGRAM entries 113..126 (BG palette 7, colors 1..14), matching <c>FadeInRidleyRoomBackground</c> at <c>$A6:A4D6</c>; does not advance the reveal timer or process its terminator.</summary>
    /// <param name="colours">The colours to update.</param>
    /// <param name="row">Zero-based reveal row, from 0 through 14; row 15 is the runtime terminator, not a color row.</param>
    public void ApplyReveal(GameplayColours colours, int row) => NorfairRidleyColors.ApplyReveal(reveal, colours, row);
}
