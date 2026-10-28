using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileCooldownDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The cooldown byte at each place of the firing table.</summary>
internal static class SamusProjectileCooldowns
{
    /// <summary>
    /// $90:C254..C28E: beam charge doubles the ordinary 15-frame delay; uncharged
    /// Plasma+Ice has its own 12-frame cadence. Held-fire repeats every 25 frames.
    /// Unused beam combinations, padding and unused non-beam kinds remain zero.
    /// </summary>
    /// <param name="offset">The byte's offset from the start of <see cref="SamusFiringTableDefinitions"/>.</param>
    internal static byte ReadByte(int offset)
    {
        int index = offset - SamusFiringTableDefinitions.UnchargedCooldowns;
        if ((uint)index < 32)
        {
            bool charged = index >= SamusFiringTableDefinitions.ChargedRowOffset;
            int combination = index & CombinationMask;
            if (combination >= SamusFiringTableDefinitions.CombinationCount)
                return 0;
            return (byte)(charged ? 30 : combination == PlasmaIce ? 12 : 15);
        }
        if (offset is >= (SamusFiringTableDefinitions.UnchargedCooldowns + 32) and < SamusFiringTableDefinitions.NonBeamCooldowns)
            return 0;
        if (offset is >= SamusFiringTableDefinitions.NonBeamCooldowns and < SamusFiringTableDefinitions.AutoFireCooldowns)
            return NonBeamCooldownRows[offset - SamusFiringTableDefinitions.NonBeamCooldowns];
        if ((uint)(offset - SamusFiringTableDefinitions.AutoFireCooldowns) < SamusFiringTableDefinitions.CombinationCount)
            return 25;
        if (offset == SamusFiringTableDefinitions.SpacetimeBeamCooldown)
            return SpacetimeCooldown;

        throw new InvalidDataException(
            $"Projectile cooldown byte at firing-table offset {offset} is outside the compiled definitions.");
    }
}
