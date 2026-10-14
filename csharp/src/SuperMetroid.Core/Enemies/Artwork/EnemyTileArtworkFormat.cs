
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Host-file geometry for native four-bit enemy tile DMA sheets.</summary>
public static class EnemyTileArtworkFormat
{
    /// <summary>File name of the versioned manifest binding native enemy-definition identities to artwork sheets.</summary>
    public const string ManifestFileName = "enemy-tiles.json";
    /// <summary>Current enemy-tile installation schema revision.</summary>
    public const int Version = 69;
    /// <summary>Stable, source-address-free name for a gunship takeoff character chunk.</summary>
    public static string GunshipLiftoffFileName(int index) =>
        $"gunship-liftoff-{index + 1}-tiles.png";
    /// <summary>All distinct ordinary graphics-set definitions in the pinned retail room states.</summary>
    public const int RetailDefinitionCount = 122;
    /// <summary>Bank-$A0 enemy-header word zero: bit 15 selects staging; bits 0..14 are tile byte length.</summary>
    public const int TileByteCountMask = 0x7fff;
    /// <summary>
    /// SHA-256 of the 122 sorted four-digit definition IDs joined with commas, independently
    /// enumerated from every bank-$B4 enemy graphics set referenced by retail room states.
    /// This makes a missing or substituted sheet fail during installation validation.
    /// </summary>
    public const string RetailDefinitionIdsSha256 =
        "8B665DEC36A4AA649CDF2327E4B7F60197D42B84534CD354347F4581D1442DD1";

    /// <summary>Creates the indexed-PNG filename for a bank-$A0 enemy-definition pointer.</summary>
    public static string FileName(EnemyDefinitionId definitionPointer) => $"enemy-{(ushort)definitionPointer:X4}-tiles.png";
    /// <summary>Creates the RGB5 palette JSON filename for a bank-$A0 enemy-definition pointer.</summary>
    public static string PaletteFileName(EnemyDefinitionId definitionPointer) => $"enemy-{(ushort)definitionPointer:X4}-colors.json";
}
