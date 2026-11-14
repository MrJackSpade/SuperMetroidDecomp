namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Identities and tables owned by the Morph-Ball bomb producer.
/// </summary>
public static class MorphBallBombDefinitions
{
    /// <summary>How many bombs Samus can have out at once.</summary>
    public const int SlotCount = 5;

    /// <summary>The wait after placing a normal bomb before Samus can place or fire again.</summary>
    public const int NormalBombCooldown = 16;

    /// <summary>The wait after placing a Power Bomb before Samus can place or fire again.</summary>
    public const int PowerBombCooldown = 40;

    /// <summary>Initial normal- and power-bomb fuse written when they are placed.</summary>
    public const int InitialBombTimer = 60;

    /// <summary>Minimum flare counter accepted by bomb or bomb-spread firing.</summary>
    public const int RequiredChargeFrames = 60;

    /// <summary>
    /// The updates of held Down a bomb spread charges for; reaching it forces release while Down remains held.
    /// </summary>
    public const int DownChargeTimeoutUpdates = 192;

    /// <summary>
    /// The updates of held Down that add one whole pixel to a bomb spread's upward launch speed: zero through three
    /// pixels over the <see cref="DownChargeTimeoutUpdates"/>.
    /// </summary>
    public const int DownChargeUpdatesPerLaunchPixel = 64;
}
