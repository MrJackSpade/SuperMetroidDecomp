namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Gameplay cues of Kraid's rock animations.</summary>
internal enum KraidRockCue
{
    /// <summary>
    /// Gives the bursting rock the frame behaviour that draws it from the room's shared sheet in its standard colours,
    /// from its next pass on.
    /// </summary>
    DrawFromSharedSheetFromNextPass,
    /// <summary>Deletes the rock.</summary>
    Delete,
}
