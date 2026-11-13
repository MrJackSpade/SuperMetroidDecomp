namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>What a cue of the Kago's bugs animations does.</summary>
internal enum KagoBugProjectileCueKind
{
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Requests the shot bug's drop.</summary>
    SpawnDrop,
    /// <summary>Settles the bug into its idle wait.</summary>
    StartIdle,
    /// <summary>Launches the bug's jump.</summary>
    StartJump,
    /// <summary>Draws the bug from the room's shared sheet in its standard colours.</summary>
    DrawFromSharedSheet,
}
