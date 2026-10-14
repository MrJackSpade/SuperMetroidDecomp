namespace SuperMetroid.Core.Game;

/// <summary>
/// The five bank-$A5 functions stored in Spore Spawn's native variable A.
/// </summary>
public enum SporeSpawnFunction : ushort
{
    /// <summary><c>RTS_A5EB1A</c> at <c>$A5:EB1A</c>; performs no main-AI movement while instruction lists control the idle or open phase.</summary>
    Idle = 0xeb1a,
    /// <summary><c>Function_SporeSpawn_Descent</c> at <c>$A5:EB1B</c>; lowers the body one room pixel per AI update until Y reaches 624 and installs the fight-start list.</summary>
    Descending = 0xeb1b,
    /// <summary><c>Function_SporeSpawn_Moving</c> at <c>$A5:EB52</c>; computes the phase-driven body sway about its spawn center and updates stalk segment positions.</summary>
    Moving = 0xeb52,
    /// <summary><c>Function_SporeSpawn_SetupDeath</c> at <c>$A5:EB9B</c>; computes the fixed death-drift angle and velocity toward room position (128,624), without moving the body.</summary>
    SetupDeath = 0xeb9b,
    /// <summary><c>Function_SporeSpawn_Dying</c> at <c>$A5:EBEE</c>; applies death drift and ceiling dust, returning to idle when both axis distances from (128,624) are below eight pixels.</summary>
    Dying = 0xebee,
}
