namespace SuperMetroid.Core.Game;

/// <summary>Named 16-bit enemy-definition pointers within cartridge enemy banks.</summary>
public static class EnemyDefinitionPointers
{
    /// <summary>Ceres Ridley's shared bank-$A6 enemy header at $A0:E13F.</summary>
    public const ushort CeresRidley = 0xe13f;

    /// <summary>EnemyHeader_Mochtroid at $A3:D8FF.</summary>
    public const ushort Mochtroid = 0xd8ff;

    /// <summary>Mother Brain's falling tube actor header at $A0:ECFF.</summary>
    public const ushort MotherBrainFallingTube = 0xecff;
}
