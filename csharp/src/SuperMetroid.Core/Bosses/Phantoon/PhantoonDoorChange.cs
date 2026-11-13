namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The changes Phantoon's fight makes to its arena's door: the only spawns Phantoon's door spawner performs.</summary>
public enum PhantoonDoorChange
{
    /// <summary>Draws the fight's door as the starting flames close in: the door's column and the cap beside it.</summary>
    FightDoor,
    /// <summary>Restores the door's own frame once Phantoon is defeated and the Wrecked Ship's power is back.</summary>
    RestoredDoor,
}
