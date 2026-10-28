namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Native NTSC firing delays, independent of projectile presentation assets.</summary>
internal static class SamusProjectileCooldownDefinitions
{
    /// <summary>The bits of a cooldown row's index that select its beam combination.</summary>
    internal const int CombinationMask = 0x0f;
    /// <summary>The SpaceTime beam's cooldown byte.</summary>
    internal const byte SpacetimeCooldown = 0x0D;

    /// <summary>Cooldown $0CCC that Handle_Samus_Cooldown ($90:AC32) holds while time is frozen.</summary>
    internal const ushort FrozenTime = 0x20;

    /// <summary>
    /// $90:C27A: the non-beam cooldowns, one row per non-beam projectile kind. The missile (row one) waits 10 frames,
    /// the super missile (row two) 20, the power bomb (row three) 40 and the morph-ball bomb (row five) 16; every
    /// other kind's row is zero.
    /// </summary>
    internal static readonly byte[] NonBeamCooldownRows = [0, 10, 20, 40, 0, 16, 0, 0, 0];
    /// <summary>Native low-nibble beam bits: Plasma plus Ice without Wave or Spazer.</summary>
    internal const int PlasmaIce = 0x0a;
}
