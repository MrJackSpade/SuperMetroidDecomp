namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>Editable initial OBJ colors and fifteen steps of fourteen arena-reveal BG colors for Lower Norfair Ridley.</summary>
public sealed class NorfairRidleyColorCatalog
{
    /// <summary>The selected initial colours and reveal steps.</summary>
    internal readonly NorfairRidleyColorSelection selection;

    /// <summary>Keeps the recognized initial and reveal paint selectors.</summary>
    /// <param name="selection">The initial colours and reveal steps the import recognized.</param>
    internal NorfairRidleyColorCatalog(NorfairRidleyColorSelection selection) => this.selection = selection;
}
