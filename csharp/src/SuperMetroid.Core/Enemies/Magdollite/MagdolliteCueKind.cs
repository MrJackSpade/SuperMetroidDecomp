namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// What a signal cue of the Magdollite's animations does: place the head and body for the throw, throw lava, mark the
/// animation busy or idle, or start the attack cooldown.
/// </summary>
internal enum MagdolliteCueKind
{
    /// <summary>Clears the animation-busy flag.</summary>
    ClearAnimationBusy,
    /// <summary>Returns the head and body to the origin height.</summary>
    RestoreOrigins,
    /// <summary>Marks the animation busy.</summary>
    SetAnimationBusy,
    /// <summary>Puts the head and body 24 pixels below the origin and shows the body and overlay.</summary>
    SinkAndShowParts,
    /// <summary>Throws a lava projectile.</summary>
    SpawnLava,
    /// <summary>Starts the 256-frame attack cooldown.</summary>
    StartCooldown,
    /// <summary>Puts the head and body 4 pixels below the origin and hides the body and overlay.</summary>
    SurfaceAndHideParts,
}
