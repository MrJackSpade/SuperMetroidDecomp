namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A6 dispatcher word stored in Nuclear Waffle variable A. “Nuclear Waffle”
/// is the disassembly's internal name; the cartridge name table calls definition $E0BF
/// Puromi.
/// </summary>
public enum NuclearWaffleEnemyFunction : ushort
{
    /// <summary>$A6:9615, Function_Puromi_Inactive: decrements the signed-expiry wait timer, then resets the sweep and joint-orientation latches and enables offscreen processing.</summary>
    Waiting = 0x9615,
    /// <summary>$A6:9682, Function_Puromi_Active: positions the head and seven interleaved links, advances angular speed, and returns to waiting when the final projectile link reaches the endpoint.</summary>
    Sweeping = 0x9682,
}
