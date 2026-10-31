using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the enemy species tables extraction reads (#1412): the import pipeline's private knowledge
/// of the cartridge, decoded once into the installed colour documents and definitions.
/// </summary>
[EncodingOwner("cartridge addresses of the enemy species colour tables")]
internal static class EnemySpeciesSourceDefinitions
{
    /// <summary>The bytes of one RGB5 colour word.</summary>
    internal const int ColorWordBytes = sizeof(ushort);

    /// <summary>Dachora's bank-$A7 colour images.</summary>
    internal static class Dachora
    {
        /// <summary>$A7:F225, the default sixteen-colour image.</summary>
        internal const int DefaultColors = 0xa7f225;

        /// <summary>$A7:F245..$A7:F2C4, the four consecutive speed-boost images.</summary>
        internal const int SpeedColors = 0xa7f245;

        /// <summary>$A7:F2C5..$A7:F344, the four consecutive stored-shine images.</summary>
        internal const int ShineColors = 0xa7f2c5;

        /// <summary>The bytes of one complete sixteen-colour image: the stride between consecutive images.</summary>
        internal const int ImageBytes = 16 * ColorWordBytes;
    }

    /// <summary>The bank-$AA n00b tube and Chozo statue colour transfers: two sixteen-colour rows each.</summary>
    internal static class ChozoAndTube
    {
        /// <summary>$AA:E2DD, the n00b tube crack initializer $AA:E716's 32 colours.</summary>
        internal const int TubeCracksColors = 0xaae2dd;

        /// <summary>$AA:E31D, the Wrecked Ship Chozo initializer $AA:E75C's 32 colours.</summary>
        internal const int WreckedShipColors = 0xaae31d;

        /// <summary>$AA:E35D, the Lower Norfair Chozo initializer $AA:E784's 32 colours.</summary>
        internal const int LowerNorfairColors = 0xaae35d;
    }

    /// <summary>Magdollite's bank-$A8 colour-cycle rows.</summary>
    internal static class Magdollite
    {
        /// <summary>$A8:AC1C, four complete sixteen-colour rows the graphics-drawn hook $A8:B0B2 copies colours nine to twelve from.</summary>
        internal const int CycleColors = 0xa8ac1c;

        /// <summary>The colours in each source row: the stride between consecutive rows, in colours.</summary>
        internal const int RowColors = 16;
    }
}
