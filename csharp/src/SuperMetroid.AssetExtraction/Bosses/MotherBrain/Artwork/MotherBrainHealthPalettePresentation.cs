using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>The installed damage-state colors for Mother Brain's final-phase body and rear legs, kept as installed.</summary>
public sealed class MotherBrainHealthPalettePresentation
{
    /// <summary>The installed body and brain colors across four health bands.</summary>
    internal readonly InstalledKeyedColourRows<MotherBrainHealthBand> body;
    /// <summary>The installed rear-leg colors across four health bands.</summary>
    internal readonly InstalledKeyedColourRows<MotherBrainHealthBand> backLegs;

    /// <summary>Keeps both complete installed colour sets.</summary>
    /// <param name="body">Each health band's body and brain colours.</param>
    /// <param name="backLegs">Each health band's rear-leg colours.</param>
    internal MotherBrainHealthPalettePresentation(InstalledKeyedColourRows<MotherBrainHealthBand> body, InstalledKeyedColourRows<MotherBrainHealthBand> backLegs)
    {
        this.body = body;
        this.backLegs = backLegs;
    }
}
