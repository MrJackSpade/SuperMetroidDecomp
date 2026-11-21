using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The state of one fragment of Norfair Ridley's breakup.</summary>
internal sealed partial class RidleyExplosionFragmentState
{
    /// <summary>Velocity in pixels per update: X, which drag slows toward zero, and Y, which gravity increases.</summary>
    public PixelVelocity<RoomPixels> Velocity { get; internal set; }

    /// <summary>The fragment's life; it dies on the update it runs out (#2233).</summary>
    public BossCountdown Lifetime { get; } = new();

    /// <summary>
    /// The piece of Ridley the fragment shows, chosen when it spawns (#2235): a still picture, shown on the fragment's first
    /// animation update and kept from then on.
    /// </summary>
    internal required EnemyFrameId Picture { get; init; }

    /// <summary>Whether the fragment shows its picture yet: its first animation update shows it.</summary>
    internal bool ShowsPicture { get; set; }
}
