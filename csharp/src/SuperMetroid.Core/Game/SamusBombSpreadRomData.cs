namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge identities and tables owned by the Morph-Ball bomb producer in bank $90.
/// </summary>
public static class SamusBombSpreadRomData
{
    /// <summary>Five physical bomb slots at projectile byte indices $0A-$12.</summary>
    public const int SlotCount = 5;

    /// <summary>Normal-bomb projectile type written by <c>$90:BF9D</c>.</summary>
    public const ushort NormalBombType = 0x0500;

    /// <summary>Bomb-spread projectile type written by <c>BombSpread</c> at $90:D849.</summary>
    public const ushort BombSpreadType = 0x8500;

    /// <summary>Power-bomb projectile type formed from HUD item index three.</summary>
    public const ushort PowerBombType = 0x0300;

    /// <summary>Normal-bomb cooldown selected from non-beam table entry five.</summary>
    public const ushort NormalBombCooldown = 0x0010;

    /// <summary>Power-bomb cooldown selected from non-beam table entry three.</summary>
    public const ushort PowerBombCooldown = 0x0028;

    /// <summary>Initial normal- and power-bomb fuse written by their firing routines.</summary>
    public const ushort InitialBombTimer = 60;

    /// <summary>Minimum flare counter accepted by <c>FireBombOrBombSpread</c> at $90:C0AB.</summary>
    public const ushort RequiredChargeFrames = 0x003c;

    /// <summary>
    /// Bits tested in the bomb-spread timeout word; reaching $C0 forces release while Down
    /// remains held.
    /// </summary>
    public const ushort DownChargeTimeoutMask = 0x00c0;
}
