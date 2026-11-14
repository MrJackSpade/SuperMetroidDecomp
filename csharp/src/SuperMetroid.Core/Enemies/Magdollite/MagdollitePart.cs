namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The part a Magdollite placement spawns as. A Magdollite is always placed as a head followed by its rising body and
/// then its tracking overlay. The names describe what each part does; they do not collapse the three placed enemies into
/// one actor.
/// </summary>
public enum MagdollitePart
{
    /// <summary>The head, the part Samus collides with, which chooses the facing and starts the three parts' attack.</summary>
    Head,
    /// <summary>The rising body, which grows and shrinks the lava pillar under the head.</summary>
    RisingBody,
    /// <summary>The tracking overlay, the hand that follows the pillar's top, animates the throw and spawns the lava projectiles.</summary>
    TrackingOverlay,
}
