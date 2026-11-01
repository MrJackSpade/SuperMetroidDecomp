namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Identities and tables owned by the Morph-Ball bomb producer.
/// </summary>
public static class MorphBallBombDefinitions
{
    /// <summary>How many bombs Samus can have out at once.</summary>
    public const int SlotCount = 5;

    /// <summary>Normal-bomb cooldown selected from non-beam table entry five.</summary>
    public const int NormalBombCooldown = 16;

    /// <summary>Power-bomb cooldown selected from non-beam table entry three.</summary>
    public const int PowerBombCooldown = 40;

    /// <summary>Initial normal- and power-bomb fuse written by their firing routines.</summary>
    public const int InitialBombTimer = 60;

    /// <summary>Minimum flare counter accepted by <c>FireBombOrBombSpread</c>.</summary>
    public const int RequiredChargeFrames = 60;

    /// <summary>
    /// The updates of held Down a bomb spread charges for within each <see cref="DownChargeTimeoutCycle"/> of the
    /// timeout counter; reaching it forces release while Down remains held (the game tests bits six and seven).
    /// </summary>
    public const int DownChargeTimeoutUpdates = 192;

    /// <summary>The span of the timeout counter the timeout reads a position within.</summary>
    public const int DownChargeTimeoutCycle = 256;
}
