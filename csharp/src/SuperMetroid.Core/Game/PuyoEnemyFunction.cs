namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A2 function pointers stored in Puyo's ordinary enemy variable C. Keeping
/// the ROM addresses as enum values makes a debugger display directly comparable with
/// <c>$0FAE,x</c> rather than hiding cartridge state behind invented host ordinals.
/// </summary>
public enum PuyoEnemyFunction : ushort
{
    /// <summary>$A2:9B65, <c>Function_Puyo_Grounded</c>: decrements the hop cooldown and selects an airborne hop when the countdown becomes negative.</summary>
    Grounded = 0x9b65,
    /// <summary>$A2:9B81, <c>Function_Puyo_Airborne</c>: dispatches the current hop or collision-recovery routine through the airborne function word.</summary>
    Airborne = 0x9b81,
}
