using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Editable initial OBJ colors and fifteen rows of fourteen arena-reveal BG colors for Lower Norfair Ridley.</summary>
public sealed class NorfairRidleyColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("NorfairRidleyColorCatalog-v1", content =>
        {
            Span<Bgr555> initialWords = stackalloc Bgr555[NorfairRidleyPaletteDefinitions.InitialColorCount];
            for (int color = 0; color < initialWords.Length; color++) initialWords[color] = initial.ColorAt(color);
            content.AppendColors("initial", initialWords);
            Bgr555[][] revealWords = new Bgr555[NorfairRidleyPaletteDefinitions.RevealRowCount][];
            for (int row = 0; row < revealWords.Length; row++)
            {
                revealWords[row] = new Bgr555[NorfairRidleyPaletteDefinitions.RevealColorCount];
                for (int color = 0; color < revealWords[row].Length; color++) revealWords[row][color] = reveal.ColorAt(row, color);
            }
            content.AppendColorFrames("reveal", revealWords);
        });

    /// <summary>Compiled initial OBJ colors, with shared paint relationships retained by the selector.</summary>
    internal readonly NorfairRidleyInitialPaintDefinitions initial;
    /// <summary>Fifteen ordered arena background rows, excluding the runtime terminator.</summary>
    internal readonly NorfairRidleyRevealPaintDefinitions reveal;

    /// <summary>Copies validated initial and reveal RGB5 colors into independent paint selectors.</summary>
    internal NorfairRidleyColorCatalog(Bgr555[] initial, Bgr555[][] reveal)
    {
        this.initial = new(initial);
        this.reveal = new(reveal);
    }

    /// <summary>Installs the 32 initial RGB5 colors in CGRAM entries 160..191 (OBJ palettes 2 and 3), matching the native copy from <c>$A6:E1CF</c>; subsequent palette clearing is the caller's responsibility.</summary>
    /// <param name="colours">The colours to update.</param>
    public void ApplyInitial(GameplayColours colours)
    {
        Ensure.NotNull(colours);
        for (int color = 0; color < NorfairRidleyPaletteDefinitions.InitialColorCount; color++)
            new RidleyColours(colours).NorfairInitial.Write(color, initial.ColorAt(color));
    }

    /// <summary>Copies one arena-reveal row to CGRAM entries 113..126 (BG palette 7, colors 1..14), matching <c>FadeInRidleyRoomBackground</c> at <c>$A6:A4D6</c>; does not advance the reveal timer or process its terminator.</summary>
    /// <param name="colours">The colours to update.</param>
    /// <param name="row">Zero-based reveal row, from 0 through 14; row 15 is the runtime terminator, not a color row.</param>
    public void ApplyReveal(GameplayColours colours, int row)
    {
        NorfairRidleyRevealPaintDefinitions.ValidateRow(row);
        Ensure.NotNull(colours);
        for (int color = 0; color < NorfairRidleyPaletteDefinitions.RevealColorCount; color++)
            new RidleyColours(colours).NorfairReveal.Write(color, reveal.ColorAt(row, color));
    }
}
