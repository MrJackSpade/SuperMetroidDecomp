/// <summary>
/// Native J/U WRAM identities compared by the reset-start movie desynchronization replay.
/// </summary>
/// <remarks>
/// This is intentionally a small gameplay-outcome set: dispatcher, RNG, room, Samus
/// kinematics/resources and enemy population. Widen it only where a diagnosed
/// divergence needs more evidence.
/// </remarks>
internal static class MovieDesyncMemory
{
    /// <summary>$0998: main game-state dispatcher index.</summary>
    public const int GameState = 0x0998;
    /// <summary>$05E5: live random-number word advanced by <c>GenerateRandomNumber</c>.</summary>
    public const int Random = 0x05e5;
    /// <summary>$05B6: 16-bit accepted-NMI frame counter.</summary>
    public const int NmiCounter = 0x05b6;
    /// <summary>$079B: current bank-$8F room header pointer.</summary>
    public const int Room = 0x079b;
    /// <summary>$0911/$0915: layer-1 camera X/Y.</summary>
    public const int CameraX = 0x0911, CameraY = 0x0915;

    /// <summary>$0AF6/$0AF8/$0AFA/$0AFC: Samus X, X subposition, Y, Y subposition.</summary>
    public const int SamusX = 0x0af6, SamusXFraction = 0x0af8, SamusY = 0x0afa, SamusYFraction = 0x0afc;
    /// <summary>$0A1C: Samus pose.</summary>
    public const int SamusPose = 0x0a1c;
    /// <summary>$09C2/$09C4: Samus energy and maximum energy.</summary>
    public const int Health = 0x09c2, MaxHealth = 0x09c4;
    /// <summary>$09A2/$09A4/$09A6/$09A8: equipped/collected items and beams.</summary>
    public const int EquippedItems = 0x09a2, CollectedItems = 0x09a4, EquippedBeams = 0x09a6, CollectedBeams = 0x09a8;
    /// <summary>$09C6..$09D0: missiles, supers and power bombs with their capacities.</summary>
    public const int Missiles = 0x09c6, MaxMissiles = 0x09c8, SuperMissiles = 0x09ca, MaxSuperMissiles = 0x09cc,
        PowerBombs = 0x09ce, MaxPowerBombs = 0x09d0;
    /// <summary>$09D4/$09D6: reserve-tank capacity and energy.</summary>
    public const int MaxReserve = 0x09d4, Reserve = 0x09d6;

    /// <summary>$0F78: first enemy slot; each slot is 64 bytes.</summary>
    public const int EnemyBase = 0x0f78;
    /// <summary>Enemy slot stride in bytes.</summary>
    public const int EnemyStride = 64;
    /// <summary>Offsets inside an enemy slot: species, X, X subposition, Y, Y subposition, health.</summary>
    public const int EnemyIdentityOffset = 0x00, EnemyXOffset = 0x02, EnemyXFractionOffset = 0x04,
        EnemyYOffset = 0x06, EnemyYFractionOffset = 0x08, EnemyHealthOffset = 0x14;

    /// <summary>$7E:2020: seven ten-word Ridley tail records; X/Y at +12/+14. The tail deals contact damage.</summary>
    public const int RidleyTailSegments = 0x2020, RidleyTailSegmentStride = 20,
        RidleyTailXOffset = 12, RidleyTailYOffset = 14;

    /// <summary>$80:9459: accepted-NMI controller-read entry recorded in each checkpoint.</summary>
    public const int ControllerReadBoundary = 0x809459;
}
