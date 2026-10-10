namespace SuperMetroid.Core.Frontend;

/// <summary>Map-object definitions consumed by $82:B6DD, $82:B892 and $82:BB30.</summary>
public static class FileSelectMapIconRomData
{
    /// <summary>$82:C7CB, per-area boss coordinate lists.</summary>
    public const int BossLists = 0x82c7cb;
    /// <summary>$82:C7DB, per-area missile refill coordinate lists, spritemap $0B.</summary>
    public const int MissileLists = 0x82c7db;
    /// <summary>$82:C7EB, per-area energy refill coordinate lists, spritemap $0A.</summary>
    public const int EnergyLists = 0x82c7eb;
    /// <summary>$82:C7FB, per-area map station coordinate lists, spritemap $4E.</summary>
    public const int MapStationLists = 0x82c7fb;
    /// <summary>$82:C74D, per-area elevator destination records (X/Y/spritemap).</summary>
    public const int ElevatorLists = 0x82c74d;
    /// <summary>$82:B892 changes a defeated boss marker to OBJ palette six.</summary>
    public const ushort DefeatedBossPalette = 0x0c00;
}
