namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Gameplay cues of the turret animations.</summary>
internal enum MotherBrainTurretCue
{
    /// <summary>Draws the smoke from the room's shared sheet in its standard colours.</summary>
    DrawFromSharedSheet,
    /// <summary>Stops the bullet's flight frame behaviour.</summary>
    ClearFrameBehavior,
    /// <summary>Deletes the finished smoke.</summary>
    Delete,
}
