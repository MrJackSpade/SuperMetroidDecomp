namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The part a Magdollite placement spawns as. Retail always allocates these
/// records consecutively as head, rising body, then tracking overlay. The names describe
/// ownership; they do not collapse the three physical enemy slots into one host actor.
/// </summary>
public enum MagdollitePart
{
    /// <summary>The collision-enabled head/master that chooses facing and initiates the three-slot attack.</summary>
    Head = 0,
    /// <summary>The following arm/slave-1 slot, which grows and shrinks the vertical pillar.</summary>
    RisingBody = 1,
    /// <summary>The hand/slave-2 slot, which follows the pillar cap, animates the throw, and spawns lava projectiles.</summary>
    TrackingOverlay = 2,
}
