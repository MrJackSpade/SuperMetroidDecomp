using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileCooldownDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The cooldown byte at each place of the firing table.</summary>
internal static class SamusProjectileCooldowns
{
    /// <summary>
    /// Beam charge doubles the ordinary 15-frame delay; uncharged Plasma+Ice has its own 12-frame cadence. Held-fire
    /// repeats every 25 frames. Only the authored cells answer: a Spazer and Plasma combination's cooldowns are read past
    /// the rows, which <see cref="SuperMetroid.Core.Compatibility.UnauthoredBeamFiring{TBeams}"/> reproduces (#1506).
    /// </summary>
    /// <param name="offset">The byte's offset from the start of <see cref="SamusFiringTableDefinitions"/>.</param>
    /// <exception cref="InvalidDataException">The offset is not an authored cooldown cell.</exception>
    internal static int ReadByte(int offset)
    {
        int index = offset - SamusFiringTableDefinitions.UnchargedCooldowns;
        // Each row holds one byte per table index, so a byte's place within its row is its combination.
        if ((uint)index < 2 * SamusFiringTableDefinitions.ChargedRowOffset &&
            index % SamusFiringTableDefinitions.ChargedRowOffset < SamusFiringTableDefinitions.CombinationCount)
        {
            bool charged = index >= SamusFiringTableDefinitions.ChargedRowOffset;
            int combination = index % SamusFiringTableDefinitions.ChargedRowOffset;
            return charged ? 30 : combination == PlasmaIce ? 12 : 15;
        }
        if (offset is >= SamusFiringTableDefinitions.NonBeamCooldowns and < SamusFiringTableDefinitions.AutoFireCooldowns)
            return NonBeamCooldownRows[offset - SamusFiringTableDefinitions.NonBeamCooldowns];
        if ((uint)(offset - SamusFiringTableDefinitions.AutoFireCooldowns) < SamusFiringTableDefinitions.CombinationCount)
            return 25;
        throw new InvalidDataException(
            $"Projectile cooldown byte at firing-table offset {offset} is not an authored cooldown.");
    }

    /// <summary>The cooldown a retail beam combination's <paramref name="shot"/> sets the shared shot timer to.</summary>
    /// <exception cref="InvalidDataException">The combination pairs Spazer with Plasma, which the rows do not author.</exception>
    internal static int Read(SamusBeamCombination beams, BeamShotCooldowns.Shot shot) => ReadByte(shot switch
    {
        BeamShotCooldowns.Shot.Uncharged => SamusFiringTableDefinitions.UnchargedCooldowns + beams.TableIndex,
        BeamShotCooldowns.Shot.Charged => SamusFiringTableDefinitions.UnchargedCooldowns + SamusFiringTableDefinitions.ChargedRowOffset + beams.TableIndex,
        BeamShotCooldowns.Shot.HeldFire => SamusFiringTableDefinitions.AutoFireCooldowns + beams.TableIndex,
        _ => throw new ArgumentOutOfRangeException(nameof(shot), shot, "Undefined beam shot."),
    });
}
