namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Cartridge identities and tables owned by the Morph-Ball bomb producer in bank $90.
/// </summary>
public static class MorphBallBombDefinitions
{
    /// <summary>Five physical bomb slots at projectile byte indices $0A-$12.</summary>
    public const int SlotCount = 5;

    /// <summary>Normal-bomb cooldown selected from non-beam table entry five.</summary>
    public const ushort NormalBombCooldown = 0x0010;

    /// <summary>Power-bomb cooldown selected from non-beam table entry three.</summary>
    public const ushort PowerBombCooldown = 0x0028;

    /// <summary>Initial normal- and power-bomb fuse written by their firing routines.</summary>
    public const ushort InitialBombTimer = 60;

    /// <summary>Minimum flare counter accepted by <c>FireBombOrBombSpread</c> at $90:C0AB.</summary>
    public const ushort RequiredChargeFrames = 0x003c;

    /// <summary>
    /// The updates of held Down a bomb spread charges for within each <see cref="DownChargeTimeoutCycle"/> of the
    /// timeout counter; reaching it forces release while Down remains held (the cartridge tests bits six and seven).
    /// </summary>
    public const int DownChargeTimeoutUpdates = 192;

    /// <summary>The span of the timeout counter the timeout reads a position within.</summary>
    public const int DownChargeTimeoutCycle = 256;
}
