using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable RGB5 colors for Power Bomb, Crystal Flash and Ceres explosions.</summary>
public sealed class InstalledPowerBombFixedColors
{
    /// <summary>Authored pre-explosion colors that differ from the calculated sixteen-entry stock sequence.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> preExplosion;
    /// <summary>Authored explosion colors, including later entries without a calculated stock definition.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> explosion;

    /// <summary>Retains the selected colours that differ from the calculated stock, separately for the two radius-selected sequences.</summary>
    /// <param name="preExplosion">The pre-explosion colours the import found differing from, or missing in, the calculated stock.</param>
    /// <param name="explosion">The explosion colours the import found differing from, or missing in, the calculated stock.</param>
    internal InstalledPowerBombFixedColors(Dictionary<ColourCell, RgbColor> preExplosion, Dictionary<ColourCell, RgbColor> explosion)
    {
        this.preExplosion = preExplosion;
        this.explosion = explosion;
    }

    /// <summary>Returns authored display color; phase and radius indexing remain engine-owned.</summary>
    public RgbColor Resolve(PowerBombFixedColorSequence sequence, int index)
    {
        Dictionary<ColourCell, RgbColor> colors = sequence switch
        {
            PowerBombFixedColorSequence.PreExplosion => preExplosion,
            PowerBombFixedColorSequence.Explosion => explosion,
            _ => throw new ArgumentOutOfRangeException(nameof(sequence)),
        };
        if ((uint)index >= PowerBombFixedColorFormat.Count(sequence))
            throw new ArgumentOutOfRangeException(nameof(index));
        if (colors.TryGetValue(new ColourCell(0, index), out var supplied)) return supplied;
        if (PowerBombFixedColorFormat.TryCalculateStock(sequence, index, out var calculated)) return calculated;
        throw new InvalidDataException("Power Bomb color has neither supplied content nor a calculated definition.");
    }
}
