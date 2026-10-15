namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Gameplay cues of Draygon's projectile programs.</summary>
internal enum DraygonProjectileCue
{
    /// <summary>Sticks the goop to Samus.</summary>
    StickToSamus,
    /// <summary>Spawns an enemy drop with Draygon's eye's drop chances where the goop burst.</summary>
    SpawnDrops,
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Gives the wall turret's shot its fired pre-instruction.</summary>
    Fire,
}
