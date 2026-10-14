namespace SuperMetroid.Core.Game;

/// <summary>The seven bank-$A2 Lower Norfair Rio (Holtz) parent and flame instruction lists.</summary>
internal enum LowerNorfairRioProgram : ushort
{
    /// <summary><c>InstList_Holtz_Idle_0</c> at $A2:C61A.</summary>
    Idle = 0xc61a,
    /// <summary><c>InstList_Holtz_PrepareToSwoop</c> at $A2:C630.</summary>
    PrepareToSwoop = 0xc630,
    /// <summary><c>InstList_Holtz_Swoop_Descending</c> at $A2:C65A.</summary>
    Descending = 0xc65a,
    /// <summary><c>InstList_Holtz_Swoop_Ascending_Part1</c> at $A2:C662.</summary>
    AscendingPart1 = 0xc662,
    /// <summary><c>InstList_Holtz_Swoop_Part2_0</c> at $A2:C674.</summary>
    AscendingPart2 = 0xc674,
    /// <summary><c>InstList_Holtz_SwoopCooldown</c> at $A2:C686.</summary>
    Cooldown = 0xc686,
    /// <summary><c>InstList_Holtz_Flames</c> at $A2:C6B0.</summary>
    Flames = 0xc6b0,
}
