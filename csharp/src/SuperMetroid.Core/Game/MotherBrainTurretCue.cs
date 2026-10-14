namespace SuperMetroid.Core.Game;

/// <summary>Gameplay cues of the turret programs.</summary>
internal enum MotherBrainTurretCue
{
    /// <summary>Draws the smoke with palette zero.</summary>
    UsePaletteZero,
    /// <summary>Stops the bullet's flight pre-instruction.</summary>
    ClearPreInstruction,
    /// <summary>Deletes the finished smoke.</summary>
    Delete,
}
