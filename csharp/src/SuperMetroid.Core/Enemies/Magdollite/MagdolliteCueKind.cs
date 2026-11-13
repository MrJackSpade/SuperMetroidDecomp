namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>What a cue of the Magdollite's animations does: move its parts, place the head and body for the throw, throw lava, mark the animation busy or idle, record a sound, or start the attack cooldown.</summary>
internal enum MagdolliteCueKind
{
    /// <summary>Clears the animation-busy flag.</summary>
    ClearAnimationBusy,
    /// <summary>Moves down two pixels.</summary>
    MoveDown2,
    /// <summary>Moves up two pixels.</summary>
    MoveUp2,
    /// <summary>Moves this part and the following one down a pixel.</summary>
    MoveWithPillarDown1,
    /// <summary>Moves this part and the following one up a pixel.</summary>
    MoveWithPillarUp1,
    /// <summary>Records the operand's enemy-library sound while the Magdollite is on screen.</summary>
    PlaySoundIfOnScreen,
    /// <summary>Returns the head and body to the origin height.</summary>
    RestoreOrigins,
    /// <summary>Marks the animation busy.</summary>
    SetAnimationBusy,
    /// <summary>Moves to 8 pixels left of and 4 above the throw origin.</summary>
    ShiftLeftUp4,
    /// <summary>Moves to 8 pixels left of and 4 above the throw origin (the second, identical left cue).</summary>
    ShiftLeftUp4Again,
    /// <summary>Moves to 8 pixels right of and 4 above the throw origin.</summary>
    ShiftRightUp4,
    /// <summary>Moves to 8 pixels right of and 8 above the throw origin.</summary>
    ShiftRightUp8,
    /// <summary>Puts the head and body 24 pixels below the origin and shows the body and overlay.</summary>
    SinkAndShowParts,
    /// <summary>Throws a lava projectile.</summary>
    SpawnLava,
    /// <summary>Starts the 256-frame attack cooldown.</summary>
    StartCooldown,
    /// <summary>Puts the head and body 4 pixels below the origin and hides the body and overlay.</summary>
    SurfaceAndHideParts,
}
