using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The shades the import pre-colours Crocomire's sheets in (#1512): his body's fight colours and hurt flash on the background,
/// the wall and projectile colours his fight loads, and his skeleton's arm and the wall spikes as he dies.
/// </summary>
public static class CrocomireShades
{
    /// <summary>His body's and head's fight colours on the background.</summary>
    public static PictureShade FightBody { get; } = new("crocomire-fight-body", 0);

    /// <summary>His body's and head's hurt flash on the background: white.</summary>
    public static PictureShade BodyHurt { get; } = new("crocomire-body-hurt", 0);

    /// <summary>The wall's colours, which the collapsing bridge's fragments also draw in.</summary>
    public static PictureShade Wall { get; } = new("crocomire-wall", 0);

    /// <summary>His projectiles' colours, which his fight loads into the projectiles' row.</summary>
    public static PictureShade Projectiles { get; } = new("crocomire-projectiles", 0);

    /// <summary>His skeleton's arm, as the wall breaks.</summary>
    public static PictureShade SkeletonArm { get; } = new("crocomire-skeleton-arm", 0);

    /// <summary>The wall's spikes, at the end of the wall's rumble.</summary>
    public static PictureShade WallSpikes { get; } = new("crocomire-wall-spikes", 0);
}
