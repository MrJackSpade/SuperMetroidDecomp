namespace SuperMetroid.Core.Game;

/// <summary>Cartridge definitions used when an actor changes lifecycle identity.</summary>
public static class EnemyLifecycleDefinitions
{
    /// <summary>$A0:DAFF, EnemyHeaders_Respawn: reserves a killed actor's slot until respawn; its AI is inert.</summary>
    public const ushort RespawnPlaceholder = 0xdaff;

    /// <summary>
    /// WRAM $003A: <c>EnemyHeaders_dropChances</c> ($86:F120) indexed by header pointer zero
    /// reads $A0:003A, which LoROM mirrors to this unnamed direct-page word. A death explosion
    /// spawned from a cleared enemy slot takes its drop-chance pointer from here.
    /// </summary>
    public const int ClearedHeaderDropChancesAddress = 0x003a;
}
