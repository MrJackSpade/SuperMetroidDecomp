using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What a projectile kind's starting damage is (<see cref="SamusProjectileDamageDefinitions.StartingDamage"/>): a beam
/// shot's, from the beams it fires and whether it is charged, or the kind's own.
/// </summary>
internal abstract partial record SamusProjectileStartingDamage
{
    /// <summary>Only the two kinds of starting damage below derive from this.</summary>
    private protected SamusProjectileStartingDamage()
    {
    }

    /// <summary>The damage a projectile of the kind starts with.</summary>
    internal abstract int Damage { get; }

    /// <summary>A beam shot of <paramref name="Beams"/>: its beams' uncharged damage, tripled when it is charged.</summary>
    /// <param name="Beams">The beams the shot fires.</param>
    /// <param name="Charged">Whether the shot is charged.</param>
    [IndependentFlag("whether a beam shot is charged is a fact of its kind, independent of the beams it fires")]
    internal sealed partial record BeamShot(SamusBeamCombination Beams, bool Charged) : SamusProjectileStartingDamage
    {
        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">The beams have no beam shot kind of their own.</exception>
        internal override int Damage
        {
            get
            {
                int uncharged = SamusProjectileDamageDefinitions.UnchargedBeamDamage.TryGetValue(Beams, out int damage)
                    ? damage
                    : throw new InvalidOperationException($"{Beams} has no beam shot kind of its own.");
                return Charged ? uncharged * SamusProjectileDamageDefinitions.ChargedBeamDamageMultiplier : uncharged;
            }
        }
    }

    /// <summary>A kind that is no beam shot, with its own starting damage.</summary>
    /// <param name="Value">The damage the kind starts with.</param>
    internal sealed partial record Own(int Value) : SamusProjectileStartingDamage
    {
        /// <inheritdoc/>
        internal override int Damage => Value;
    }
}
