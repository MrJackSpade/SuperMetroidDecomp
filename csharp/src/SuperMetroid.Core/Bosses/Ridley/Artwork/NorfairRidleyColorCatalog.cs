using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Editable initial OBJ colors and fifteen steps of fourteen arena-reveal BG colors for Lower Norfair Ridley.</summary>
public sealed class NorfairRidleyColorCatalog
{
    /// <summary>The selected initial colours and reveal steps.</summary>
    internal readonly NorfairRidleyColorSelection selection;

    /// <summary>Keeps the recognized initial and reveal paint selectors.</summary>
    /// <param name="selection">The initial colours and reveal steps the import recognized.</param>
    internal NorfairRidleyColorCatalog(NorfairRidleyColorSelection selection) => this.selection = selection;

    /// <summary>Installs the 32 initial RGB5 colors in colours 160..191 (sprite palettes 2 and 3), matching the copy; subsequent palette clearing is the caller's responsibility.</summary>
    /// <param name="colours">The colours to update.</param>
    public void ApplyInitial(GameplayColours colours) => NorfairRidleyColors.ApplyInitial(selection.Initial, colours);

    /// <summary>Copies one arena-reveal step to colours 113..126 (background palette 7, colors 1..14), matching <c>FadeInRidleyRoomBackground</c>; does not advance the reveal timer or process its terminator.</summary>
    /// <param name="colours">The colours to update.</param>
    /// <param name="step">Zero-based reveal step, from 0 through 14; step 15 is the runtime terminator, not a color step.</param>
    public void ApplyReveal(GameplayColours colours, int step) => NorfairRidleyColors.ApplyReveal(selection.Reveal, colours, step);
}
