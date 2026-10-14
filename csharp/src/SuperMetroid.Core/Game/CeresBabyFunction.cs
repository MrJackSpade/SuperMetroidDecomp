namespace SuperMetroid.Core.Game;

/// <summary>The Ceres Baby Metroid's bank-$A6 behavior function, kept in Ridley's extended workspace.</summary>
public enum CeresBabyFunction : ushort
{
    /// <summary>Outside Ceres the Baby workspace is never initialized and holds zero.</summary>
    None = 0,
    /// <summary><c>UpdateBabyMetroidPosition_CarriedInArms</c> at $A6:BE9C.</summary>
    CarriedInArms = 0xbe9c,
    /// <summary><c>UpdateBabyMetroidPosition_CarriedInFeet</c> at $A6:BEB3.</summary>
    CarriedInFeet = 0xbeb3,
    /// <summary><c>DropBabyMetroid</c> at $A6:BECA.</summary>
    Drop = 0xbeca,
    /// <summary><c>BabyMetroidDropped</c> at $A6:BEDC.</summary>
    Dropped = 0xbedc,
    /// <summary><c>RTS_A6BF19</c> at $A6:BF19: the idle function after landing.</summary>
    Idle = 0xbf19,
}
