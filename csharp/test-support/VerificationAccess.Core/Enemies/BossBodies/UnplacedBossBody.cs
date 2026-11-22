namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A body no boss places, for a check that attaches a body of parts alone: the room shows it unplaced and never hides it
/// or the room's background.
/// </summary>
internal sealed class UnplacedBossBody : IBossBodyShowing
{
    /// <summary>The one unplaced body.</summary>
    private static readonly UnplacedBossBody Instance = new();

    /// <summary>Only <see cref="Instance"/> exists.</summary>
    private UnplacedBossBody()
    {
    }

    /// <inheritdoc/>
    public Spatial.Position16<Spatial.RoomPixels>? BodyPlacement => null;

    /// <inheritdoc/>
    public bool BodyHidden => false;

    /// <summary><paramref name="parts"/> as the room shows a body no boss places.</summary>
    internal static PlacedBossBody Showing(BossBodyParts parts) => new(parts, Instance);
}
