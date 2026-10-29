using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Editable initial OBJ colors and fifteen steps of fourteen arena-reveal BG colors for Lower Norfair Ridley.</summary>
public sealed class NorfairRidleyColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered steps, independent of JSON encoding.</summary>
    public string ContentIdentity => NorfairRidleyColors.ContentIdentity(this);

    /// <summary>The selected initial colours and reveal steps.</summary>
    internal readonly NorfairRidleyColorSelection selection;

    /// <summary>Copies validated initial and reveal RGB5 colors into independent paint selectors.</summary>
    internal NorfairRidleyColorCatalog(RgbColor[] initial, RgbColor[][] reveal) =>
        selection = new(new NorfairRidleyInitialPaint(initial), new NorfairRidleyRevealPaint(reveal));

    /// <summary>Installs the 32 initial RGB5 colors in CGRAM entries 160..191 (OBJ palettes 2 and 3), matching the native copy from <c>$A6:E1CF</c>; subsequent palette clearing is the caller's responsibility.</summary>
    /// <param name="colours">The colours to update.</param>
    public void ApplyInitial(GameplayColours colours) => NorfairRidleyColors.ApplyInitial(selection.Initial, colours);

    /// <summary>Copies one arena-reveal step to CGRAM entries 113..126 (BG palette 7, colors 1..14), matching <c>FadeInRidleyRoomBackground</c> at <c>$A6:A4D6</c>; does not advance the reveal timer or process its terminator.</summary>
    /// <param name="colours">The colours to update.</param>
    /// <param name="step">Zero-based reveal step, from 0 through 14; step 15 is the runtime terminator, not a color step.</param>
    public void ApplyReveal(GameplayColours colours, int step) => NorfairRidleyColors.ApplyReveal(selection.Reveal, colours, step);
}
