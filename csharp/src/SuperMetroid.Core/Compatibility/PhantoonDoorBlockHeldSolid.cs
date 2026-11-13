namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Each of Phantoon's door changes (#1678) turns the top block of the door's transition column into a plain solid block
/// as it is spawned, keeping its picture, and the block stays solid until the change's first update draws the door
/// again: for that one update Samus meets a wall where the door's transition starts, and a shot meets a solid block.
/// </summary>
/// <remarks>
/// Consulted by Phantoon's door spawner, which deactivates the block right after the change takes its room-object place
/// on the enemy pass that asks for it (the end of the eighth starting flame, and the end of the Wrecked Ship's power
/// coming back), and by the changes' timing, whose first frame draws after <see cref="HeldUpdates"/>. Evidence: both of
/// Phantoon's hard-coded door spawns run the shared deactivating setup on the block they are spawned at, which keeps
/// only the solid bit of the block's collision kind, so the door's transition block becomes a solid block; their
/// animations draw the door on their first update and end on the next.
/// </remarks>
[CompatibilityService("each of Phantoon's door changes makes the door's top transition block solid from its spawn until its first update draws the door")]
public static class PhantoonDoorBlockHeldSolid
{
    /// <summary>The room-object updates the door's top block stays solid: from the change's spawn to its first update.</summary>
    public const int HeldUpdates = 1;
}
