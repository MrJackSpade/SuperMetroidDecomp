namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Morph Ball's bombs: how many she can place, their fuses and the waits after them, and the charge and held Down a
/// bomb spread takes.
/// </summary>
public static partial class MorphBallBombDefinitions
{
    /// <summary>How many bombs Samus can have out at once.</summary>
    public const int PlaceCount = 5;

    /// <summary>The wait after placing a normal bomb before Samus can place or fire again.</summary>
    public const int NormalBombCooldown = 16;

    /// <summary>The wait after placing a Power Bomb before Samus can place or fire again.</summary>
    public const int PowerBombCooldown = 40;

    /// <summary>Initial normal- and power-bomb fuse written when they are placed.</summary>
    public const int InitialBombTimer = 60;

    /// <summary>
    /// The blocks a normal bomb's explosion reacts with, from the block holding its centre, in the order it reacts with
    /// them: the centre, then above, right, left and below.
    /// </summary>
    public static IReadOnlyList<Offset16<RoomBlocks>> ExplosionCross { get; } = [new(0, 0), new(0, -1), new(1, 0), new(-1, 0), new(0, 1)];

    /// <summary>
    /// The updates of held Down a bomb spread charges for; reaching it forces release while Down remains held.
    /// </summary>
    public const int DownChargeTimeoutUpdates = 192;

    /// <summary>The updates a bomb's reservation lengthens the shared projectile cooldown by, before its placement replaces it.</summary>
    internal const int ReservationCooldownUpdates = 1;

    /// <summary>
    /// The updates of held Down that add one whole pixel to a bomb spread's upward launch speed: zero through three
    /// pixels over the <see cref="DownChargeTimeoutUpdates"/>.
    /// </summary>
    public const int DownChargeUpdatesPerLaunchPixel = 64;

    /// <summary>The explosion count at which a bomb or Power Bomb launches a Samus it overlaps into a bomb jump.</summary>
    public const int BombJumpExplosionCount = 8;
}
