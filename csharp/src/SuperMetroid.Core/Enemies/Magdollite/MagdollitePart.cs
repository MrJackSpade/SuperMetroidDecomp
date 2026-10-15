namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The role encoded by Magdollite population parameter one. Retail always allocates these
/// records consecutively as head, rising body, then tracking overlay. The names describe
/// ownership; they do not collapse the three physical enemy slots into one host actor.
/// </summary>
public enum MagdollitePart : ushort
{
    /// <summary>Population parameter 0: the collision-enabled head/master that chooses facing and initiates the three-slot attack.</summary>
    Head = 0,
    /// <summary>Population parameter 1: the following arm/slave-1 slot, which grows and shrinks the vertical pillar.</summary>
    RisingBody = 1,
    /// <summary>Population parameter 2: the hand/slave-2 slot, which follows the pillar cap, animates the throw, and spawns lava projectiles.</summary>
    TrackingOverlay = 2,
}
