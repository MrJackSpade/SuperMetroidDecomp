namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values and helpers of <see cref="KraidFootCollisionDefinitions"/> that only verification reads.</summary>
internal static class KraidFootCollisionDefinitionsConstants
{
    /// <summary>The shared single-rectangle hitbox list at $A7:9453.</summary>
    internal const ushort HitboxList = 0x9453;
    /// <summary>$A7:9453 contains one centered six-pixel-radius rectangle with ordinary touch and inert shot reactions.</summary>
    internal static System.Collections.Immutable.ImmutableArray<KraidFootCollisionHitbox> HitboxesAt(ushort pointer) =>
        pointer == HitboxList ? KraidFootCollisionDefinitions.SharedHitboxes : throw new InvalidDataException(
            $"Kraid-foot hitbox list $A7:{pointer:X4} is not compiled.");
}
