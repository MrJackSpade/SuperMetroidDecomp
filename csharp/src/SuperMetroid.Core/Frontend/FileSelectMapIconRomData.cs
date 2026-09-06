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
    /// <summary>$82:B892 uses boss marker spritemap nine.</summary>
    public const ushort Boss = 9;
    /// <summary>$82:B892 overlays defeated bosses with spritemap $62.</summary>
    public const ushort DefeatedBoss = 0x62;
    /// <summary>$82:B6DD uses spritemap $0B for missile stations.</summary>
    public const ushort Missile = 0x0b;
    /// <summary>$82:B6DD uses spritemap $0A for energy stations.</summary>
    public const ushort Energy = 0x0a;
    /// <summary>$82:B6DD uses spritemap $4E for map stations.</summary>
    public const ushort MapStation = 0x4e;
    /// <summary>$82:B6DD adds spritemap $63 at Crateria's first save-point coordinate.</summary>
    public const ushort Gunship = 0x63;
    /// <summary>$82:B892 changes a defeated boss marker to OBJ palette six.</summary>
    public const ushort DefeatedBossPalette = 0x0c00;
}
