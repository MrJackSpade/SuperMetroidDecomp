namespace Game.Tests.Movies;

/// <summary>
/// Where the cartridge keeps, in its memory, the things a movie's check reads from a record: the oracle's addresses.
/// They are named here for the check alone; the game knows none of them.
/// </summary>
public static class Cartridge
{
    /// <summary>The count of the cartridge's own frames.</summary>
    public const int NmiCounter = 0x05b6;

    /// <summary>The random number.</summary>
    public const int Random = 0x05e5;

    /// <summary>The states of the sound handler's three libraries, a byte each: Samus's, the enemies', the room's.</summary>
    public const int SoundStates = 0x0649, SoundLibraries = 3;

    /// <summary>The room, by the address of its header.</summary>
    public const int Room = 0x079b;

    /// <summary>The view's left edge: its part of a pixel, then its whole pixels; and the same of its top edge.</summary>
    public const int ViewXPart = 0x090f, ViewX = 0x0911, ViewYPart = 0x0913, ViewY = 0x0915;

    /// <summary>The escape's time left.</summary>
    public const int EscapeTime = 0x0943;

    /// <summary>What kind of moment it is: the title, play, a pause, a door, a message and the rest.</summary>
    public const int GameState = 0x0998;

    /// <summary>Equipment switched on and owned, then beams switched on and owned, as bits.</summary>
    public const int EquipmentOn = 0x09a2, EquipmentOwned = 0x09a4, BeamsOn = 0x09a6, BeamsOwned = 0x09a8;

    /// <summary>Energy and its capacity; missiles, super missiles and power bombs, each with its capacity; and the reserves' capacity and what they hold.</summary>
    public const int Energy = 0x09c2, EnergyCapacity = 0x09c4, Missiles = 0x09c6, MissileCapacity = 0x09c8,
        SuperMissiles = 0x09ca, SuperMissileCapacity = 0x09cc, PowerBombs = 0x09ce, PowerBombCapacity = 0x09d0,
        ReserveCapacity = 0x09d4, Reserve = 0x09d6;

    /// <summary>Samus's place: whole pixels across, the part of a pixel across, whole pixels down, the part of a pixel down.</summary>
    public const int SamusX = 0x0af6, SamusXPart = 0x0af8, SamusY = 0x0afa, SamusYPart = 0x0afc;

    /// <summary>The enemies' places in the cartridge's list: where the first begins, how far apart they lie, how many there are, and how much of each a record keeps.</summary>
    public const int Enemies = 0x0f78, EnemyStride = 64, EnemyPlaces = 32, EnemyCellsKept = 0x1e;

    /// <summary>Within an enemy's place: its kind (nought for an empty place), where it is, and its health.</summary>
    public const int EnemyKind = 0x00, EnemyX = 0x02, EnemyXPart = 0x04, EnemyY = 0x06, EnemyYPart = 0x08, EnemyHealth = 0x14;

    /// <summary>Which way Samus faces: 4 for the left, 8 for the right. Kept by the wider layout and the starting memory only.</summary>
    public const int SamusFacing = 0x0a1e;

    /// <summary>The game state of ordinary play, and of the pause's screens once they are up.</summary>
    public const int InPlay = 0x08, Paused = 0x0f;

    /// <summary>What enemies have launched: each one's kind (nought for an empty place), and where it is; and how many places there are.</summary>
    public const int LaunchedKinds = 0x1997, LaunchedX = 0x1a4b, LaunchedY = 0x1a93, LaunchedPlaces = 18;
}
