namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The codec between <see cref="SamusProjectileDirection"/> and the cartridge's ten-way aim index, which numbers the
/// aim directions clockwise from up (facing right) through down to up (facing left). Core keeps the index because
/// pose-definition byte three stores it among turning and no-fire restriction bytes and adjacent-code over-reads, the
/// installed arm-cannon artwork and flare placements select their rows by it, the grapple tables extend past it into
/// adjacent rows, and a projectile's direction word holds it in its low nibble. This is the one place a direction
/// becomes its index or an index its direction.
/// </summary>
[SnesEncodingOwner]
internal static class SamusProjectileDirectionCodec
{
    /// <summary>The number of directions the aim index names.</summary>
    internal const int Count = 10;

    /// <summary>The direction's aim index.</summary>
    internal static byte ToAimIndex(this SamusProjectileDirection direction) => (byte)direction;

    /// <summary>The direction an aim index names, or null when it names none.</summary>
    internal static SamusProjectileDirection? TryFromAimIndex(int index) =>
        (uint)index < Count ? (SamusProjectileDirection)index : null;

    /// <summary>The direction an aim index names.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index names no direction.</exception>
    internal static SamusProjectileDirection FromAimIndex(int index) =>
        (uint)index < Count
            ? (SamusProjectileDirection)index
            : throw new ArgumentOutOfRangeException(nameof(index), index, "The aim index names ten directions.");
}
