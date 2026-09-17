namespace SuperMetroid.Core.Game;

/// <summary>Compiled gameplay selectors for Golden Torizo's reflected projectiles.</summary>
internal static class GoldenTorizoProjectileDefinitions
{
    /// <summary>$86:B209, instruction list for a reflected Super Missile travelling left.</summary>
    private const ushort ReflectedSuperMissileLeft = 0xb2c1;

    /// <summary>$86:B20B, instruction list for a reflected Super Missile travelling right.</summary>
    private const ushort ReflectedSuperMissileRight = 0xb293;

    /// <summary>Returns the cartridge instruction list selected by Golden Torizo's facing.</summary>
    internal static ushort GetReflectedSuperMissileInstruction(bool facingRight) =>
        facingRight ? ReflectedSuperMissileRight : ReflectedSuperMissileLeft;
}
