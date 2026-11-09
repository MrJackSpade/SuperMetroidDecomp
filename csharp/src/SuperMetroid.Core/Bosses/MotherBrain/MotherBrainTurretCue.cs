namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Gameplay cues of the turret programs.</summary>
internal enum MotherBrainTurretCue
{
    /// <summary>Draws the smoke with its alternate colours.</summary>
    UsePaletteZero,
    /// <summary>Stops the bullet's flight frame behaviour.</summary>
    ClearFrameBehavior,
    /// <summary>Deletes the finished smoke.</summary>
    Delete,
}
