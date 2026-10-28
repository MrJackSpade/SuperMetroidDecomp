namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// The routine Nuclear Waffle's main AI dispatches. “Nuclear Waffle” is the disassembly's internal
/// name; the cartridge name table calls the enemy Puromi.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum NuclearWaffleEnemyFunction
{
    /// <summary>$A6:9615, Function_Puromi_Inactive: decrements the signed-expiry wait timer, then resets the sweep and joint-orientation latches and enables offscreen processing.</summary>
    Waiting = 1,
    /// <summary>$A6:9682, Function_Puromi_Active: positions the head and seven interleaved links, advances angular speed, and returns to waiting when the final projectile link reaches the endpoint.</summary>
    Sweeping,
}
