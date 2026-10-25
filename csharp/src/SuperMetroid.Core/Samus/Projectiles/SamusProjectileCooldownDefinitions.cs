namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Native NTSC firing delays, independent of projectile presentation assets.</summary>
internal static class SamusProjectileCooldownDefinitions
{
    /// <summary>Cooldown $0CCC that Handle_Samus_Cooldown ($90:AC32) holds while time is frozen.</summary>
    internal const ushort FrozenTime = 0x20;

    // The $90:C27A rows of the non-beam kinds with a nonzero cooldown; the other kinds' rows are zero.

    /// <summary>The ordinary missile's non-beam cooldown row.</summary>
    private const int MissileRow = 1;
    /// <summary>The super missile's non-beam cooldown row.</summary>
    private const int SuperMissileRow = 2;
    /// <summary>The power bomb's non-beam cooldown row.</summary>
    private const int PowerBombRow = 3;
    /// <summary>The morph-ball bomb's non-beam cooldown row.</summary>
    private const int BombRow = 5;
    /// <summary>Native low-nibble beam bits: Plasma plus Ice without Wave or Spazer.</summary>
    private const int PlasmaIce = 0x0a;

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
            int combination = index & 0x0f;
            if (combination >= SamusFiringTableDefinitions.CombinationCount)
                return 0;
            return (byte)(charged ? 30 : combination == PlasmaIce ? 12 : 15);
        }
        if (offset is >= (SamusFiringTableDefinitions.UnchargedCooldowns + 32) and < SamusFiringTableDefinitions.NonBeamCooldowns)
            return 0;
        if (offset is >= SamusFiringTableDefinitions.NonBeamCooldowns and < SamusFiringTableDefinitions.AutoFireCooldowns)
        {
            // The row is the non-beam projectile kind; kinds without a cooldown are zero.
            return (offset - SamusFiringTableDefinitions.NonBeamCooldowns) switch
            {
                MissileRow => 10,
                SuperMissileRow => 20,
                PowerBombRow => 40,
                BombRow => 16,
                _ => 0,
            };
        }
        if ((uint)(offset - SamusFiringTableDefinitions.AutoFireCooldowns) < SamusFiringTableDefinitions.CombinationCount)
            return 25;
        if (offset == SamusFiringTableDefinitions.SpacetimeBeamCooldown)
            return 0x0D;

        throw new InvalidDataException(
            $"Projectile cooldown byte at firing-table offset {offset} is outside the compiled definitions.");
    }
}
