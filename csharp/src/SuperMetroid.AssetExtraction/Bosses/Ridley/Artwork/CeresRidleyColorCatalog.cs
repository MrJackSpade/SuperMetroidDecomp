using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>Editable Ceres Ridley, shared Norfair Ridley health, and private Baby draw colors.</summary>
public sealed class CeresRidleyColorCatalog
{
    /// <summary>The selected colour families.</summary>
    internal readonly CeresRidleyColorSelection selection;

    /// <summary>Retains the compiled families, with the retreat colours kept only where they differ from their owners.</summary>
    /// <param name="selection">The families the import compiled.</param>
    internal CeresRidleyColorCatalog(CeresRidleyColorSelection selection) => this.selection = selection;

    /// <summary>Returns one BGR555 retreat BG color, index 0..14, preserving supplied edits over the composition of body-fade shade one, eye-fade shade fourteen, and the neutral tail colors.</summary>
    public RgbColor ResolveRetreatBg(int color) => CeresRidleyColors.RetreatBg(this, color);
    /// <summary>Returns one retreat shared color, index 0..7; unedited values reuse startup colors 1..8, as the retreat's palette copy does.</summary>
    public RgbColor ResolveRetreatShared(int color) => CeresRidleyColors.RetreatShared(this, color);
}
