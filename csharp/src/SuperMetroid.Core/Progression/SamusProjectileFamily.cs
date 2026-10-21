namespace SuperMetroid.Core.Progression;

/// <summary>Verified projectile-family values stored in bits 8–11 of WRAM <c>$0C18</c>.</summary>
/// <remarks>
/// Family <c>$4</c> remains unnamed because the translated paths do not yet establish its
/// producer. Omitting it is intentional; casting its raw nibble to this enum still preserves
/// the value for diagnostics without blessing a guess as domain terminology.
/// </remarks>
public enum SamusProjectileFamily : ushort
{
    /// <summary>Beam family, including charged and uncharged beam payload bits.</summary>
    Beam = 0x0000,
    /// <summary>Ordinary missile family.</summary>
    Missile = 0x0100,
    /// <summary>Super Missile family.</summary>
    SuperMissile = 0x0200,
    /// <summary>Power Bomb family.</summary>
    PowerBomb = 0x0300,
    /// <summary>Ordinary bomb family.</summary>
    Bomb = 0x0500,
    /// <summary>Beam-impact explosion family.</summary>
    BeamExplosion = 0x0700,
    /// <summary>Missile-impact explosion family.</summary>
    MissileExplosion = 0x0800,
}
