namespace SuperMetroid.Core.Game;

/// <summary>Six authored spawn records used by gunship liftoff dust.</summary>
public static class GunshipDustDefinitions
{
    /// <summary>$86:A2C4, InitAI_EnemyProjectile_GunshipLiftoffDustClouds: dust is eighty pixels below Samus.</summary>
    public const ushort YOffset = 0x50;

    /// <summary>
    /// $86:A2D6..A2ED, .Xoffsets and .InstListPointers in
    /// InitAI_EnemyProjectile_GunshipLiftoffDustClouds. Even parameters 0/2/4 select
    /// right-side clouds; 6/8/A select left-side clouds. Lists are the corresponding
    /// InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index*_0 programs.
    /// </summary>
    public static (short XOffset, ushort Instruction) ForParameter(ushort parameter) => parameter switch
    {
        0 => (64, 0xa197),
        2 => (72, 0xa1c1),
        4 => (80, 0xa1eb),
        6 => (-64, 0xa211),
        8 => (-72, 0xa23b),
        10 => (-80, 0xa265),
        _ => throw new ArgumentOutOfRangeException(nameof(parameter), parameter, "Gunship dust parameter must be 0,2,4,6,8,A."),
    };
}
