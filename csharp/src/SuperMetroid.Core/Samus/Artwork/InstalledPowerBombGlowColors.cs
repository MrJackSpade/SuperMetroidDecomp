using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The installed glow colours a Power Bomb, Crystal Flash or Ceres station explosion adds to the scene inside its window, one per
/// radius step of each sequence. The import resolves every entry; the explosion selects one by its radius and fades
/// it in its own state.
/// </summary>
public sealed class InstalledPowerBombGlowColors
{
    /// <summary>The pre-explosion flash's colours, indexed by its radius step.</summary>
    private readonly RgbColor[] preExplosion;

    /// <summary>The damaging explosion's colours, indexed by its radius step.</summary>
    private readonly RgbColor[] explosion;

    /// <summary>Keeps both complete sequences, checking each holds one colour per radius step.</summary>
    /// <param name="preExplosion">Every pre-explosion colour, in radius-step order.</param>
    /// <param name="explosion">Every explosion colour, in radius-step order.</param>
    /// <exception cref="InvalidDataException">A sequence does not hold exactly its radius-step count.</exception>
    internal InstalledPowerBombGlowColors(RgbColor[] preExplosion, RgbColor[] explosion)
    {
        if (preExplosion.Length != PowerBombGlowColorFormat.Count(PowerBombGlowSequence.PreExplosion) ||
            explosion.Length != PowerBombGlowColorFormat.Count(PowerBombGlowSequence.Explosion))
            throw new InvalidDataException("Power Bomb explosion colours require one colour per radius step of each sequence.");
        this.preExplosion = preExplosion;
        this.explosion = explosion;
    }

    /// <summary>Returns the colour a sequence adds at a radius step; phase and radius selection stay with the explosion.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sequence is undefined or the step is outside it.</exception>
    public RgbColor Resolve(PowerBombGlowSequence sequence, int index)
    {
        RgbColor[] colors = sequence switch
        {
            PowerBombGlowSequence.PreExplosion => preExplosion,
            PowerBombGlowSequence.Explosion => explosion,
            _ => throw new ArgumentOutOfRangeException(nameof(sequence)),
        };
        if ((uint)index >= (uint)colors.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colors[index];
    }
}
