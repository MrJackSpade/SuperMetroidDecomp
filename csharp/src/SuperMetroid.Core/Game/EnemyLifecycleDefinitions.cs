namespace SuperMetroid.Core.Game;

/// <summary>Cartridge definitions used when an actor changes lifecycle identity.</summary>
public static class EnemyLifecycleDefinitions
{
    /// <summary>$A0:DAFF, EnemyHeaders_Respawn: reserves a killed actor's slot until respawn; its AI is inert.</summary>
    public const ushort RespawnPlaceholder = 0xdaff;
}
