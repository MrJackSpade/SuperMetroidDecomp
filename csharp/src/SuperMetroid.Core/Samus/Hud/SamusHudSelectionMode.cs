namespace SuperMetroid.Core.Samus.Hud;

/// <summary>How the selected HUD item fires in each kind of movement: which weapons a movement admits and whether it keeps the charge.</summary>
internal enum SamusHudSelectionMode
{
    /// <summary>Standing, running, ordinary jumps and falls: every weapon fires.</summary>
    Standard,
    /// <summary>The Morph Ball: bombs and Power Bombs instead of beams.</summary>
    MorphBall,
    /// <summary>Spin and wall jumps, knockback, shinespark, Crystal Flash and similar poses: an inactive Grapple preserves the charge.</summary>
    Jump,
    /// <summary>Grappling.</summary>
    Grapple,
    /// <summary>Held by Draygon; all held poses use normal weapons.</summary>
    DraygonHeld,
    /// <summary>Turning.</summary>
    Turning,
    /// <summary>Changing posture.</summary>
    Transition,
}
