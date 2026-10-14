namespace SuperMetroid.Core.Game;

/// <summary>Retail populations used to validate Ripper-family OAM mode.</summary>
internal static class EnemyRomTablePointersRipper
{
    /// <summary>GRipper room-population record at $A1:B16D.</summary>
    public const int GRipperPopulationRecord = 0xa1b16d;
    /// <summary>Ripper II room-population record at $A1:A48B.</summary>
    public const int Ripper2PopulationRecord = 0xa1a48b;
    /// <summary>Ordinary Ripper room-population record at $A1:9452.</summary>
    public const int RipperPopulationRecord = 0xa19452;
}
