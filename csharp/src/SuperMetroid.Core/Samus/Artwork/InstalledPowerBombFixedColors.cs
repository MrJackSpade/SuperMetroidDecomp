using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The installed colours a Power Bomb, Crystal Flash or Ceres station explosion adds inside its window, one per
/// radius step of each sequence. The import resolves every entry; the explosion selects one by its radius and fades
/// it in its own state.
/// </summary>
public sealed class InstalledPowerBombFixedColors
{
    /// <summary>The pre-explosion flash's colours, indexed by its radius step.</summary>
    private readonly RgbColor[] preExplosion;

    /// <summary>The damaging explosion's colours, indexed by its radius step.</summary>
    private readonly RgbColor[] explosion;

    /// <summary>Keeps both complete sequences, checking each holds one colour per radius step.</summary>
    /// <param name="preExplosion">Every pre-explosion colour, in radius-step order.</param>
    /// <param name="explosion">Every explosion colour, in radius-step order.</param>
    /// <exception cref="InvalidDataException">A sequence does not hold exactly its radius-step count.</exception>
    internal InstalledPowerBombFixedColors(RgbColor[] preExplosion, RgbColor[] explosion)
    {
        if (preExplosion.Length != PowerBombFixedColorFormat.Count(PowerBombFixedColorSequence.PreExplosion) ||
            explosion.Length != PowerBombFixedColorFormat.Count(PowerBombFixedColorSequence.Explosion))
            throw new InvalidDataException("Power Bomb explosion colours require one colour per radius step of each sequence.");
        this.preExplosion = preExplosion;
        this.explosion = explosion;
    }

    /// <summary>Returns the colour a sequence adds at a radius step; phase and radius selection stay with the explosion.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sequence is undefined or the step is outside it.</exception>
    public RgbColor Resolve(PowerBombFixedColorSequence sequence, int index)
    {
        RgbColor[] colors = sequence switch
        {
            PowerBombFixedColorSequence.PreExplosion => preExplosion,
            PowerBombFixedColorSequence.Explosion => explosion,
            _ => throw new ArgumentOutOfRangeException(nameof(sequence)),
        };
        if ((uint)index >= (uint)colors.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colors[index];
    }
}
