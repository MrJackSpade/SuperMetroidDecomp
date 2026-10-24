using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable RGB5 colors for Power Bomb, Crystal Flash and Ceres explosions.</summary>
public sealed class PowerBombFixedColorCatalog
{
    /// <summary>Authored pre-explosion colors that differ from the calculated sixteen-entry stock sequence.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> preExplosion;
    /// <summary>Authored explosion colors, including later entries without a calculated stock definition.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> explosion;

    /// <summary>Retains the selected colours that differ from the calculated stock, separately for the two radius-selected sequences.</summary>
    /// <param name="preExplosion">The selected pre-explosion colours.</param>
    /// <param name="explosion">The selected explosion colours.</param>
    internal PowerBombFixedColorCatalog(Bgr555[] preExplosion, Bgr555[] explosion)
    {
        this.preExplosion = Edits(preExplosion, PowerBombFixedColorSequence.PreExplosion);
        this.explosion = Edits(explosion, PowerBombFixedColorSequence.Explosion);
    }

    /// <summary>Keeps the entries that differ from the calculated stock sequence, and every entry stock does not calculate.</summary>
    private static Dictionary<ColourCell, Bgr555> Edits(Bgr555[] supplied, PowerBombFixedColorSequence sequence)
    {
        var colors = new Dictionary<ColourCell, Bgr555>();
        for (int index = 0; index < supplied.Length; index++)
            if (!PowerBombFixedColorFormat.TryCalculateStock(sequence, index, out var calculated) || supplied[index] != calculated)
                colors.Add(new ColourCell(0, index), supplied[index]);
        return colors;
    }

    /// <summary>Returns authored display color; phase and radius indexing remain engine-owned.</summary>
    public Bgr555 Resolve(PowerBombFixedColorSequence sequence, int index)
    {
        Dictionary<ColourCell, Bgr555> colors = sequence switch
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
