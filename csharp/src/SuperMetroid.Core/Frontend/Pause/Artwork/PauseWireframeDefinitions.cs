namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Native pause wireframe patch geometry, independent of inventory selection.</summary>
public static class PauseWireframeDefinitions
{
    /// <summary>The supported editable pause-wireframe JSON schema version.</summary>
    public const int Version = 1;

    /// <summary>The embedded editable pause-wireframe asset file name.</summary>
    public const string FileName = "pause-wireframes.json";

    /// <summary>The four complete Power/Varia and regular/Hi-Jump artwork variants.</summary>
    public const int Count = 4;
    /// <summary>$82:B20C copies seventeen rows of eight words from bank $82.</summary>
    public const int Columns = 8, Rows = 17, Cells = Columns * Rows;
    /// <summary>$82:B20C starts the wireframe at tile (12,7) of the equipment page.</summary>
    public const int FirstPageTile = 7 * PageColumns + 12;
    /// <summary>The tiles in one row of the equipment page.</summary>
    public const int PageColumns = 32;
    /// <summary>The tiles of the equipment page, one 32x32 tilemap.</summary>
    public const int PageTiles = PageColumns * 32;
    /// <summary>Native packed body-piece rows advance sixteen BG characters within the pause interface artwork.</summary>
    internal const int PieceRowStride = 16;
    /// <summary>$82:D527 and corresponding variants: left helmet half starts at tile $1B3.</summary>
    internal const int Helmet = 0x1b3;
    /// <summary>$82:D531/$D751: Power/Varia shoulder pieces begin at $1BC/$1C0.</summary>
    internal const int PowerShoulder = 0x1bc, VariaShoulder = 0x1c0;
    /// <summary>$82:D571 and corresponding variants: common left arm strip starts at $170.</summary>
    internal const int Arm = 0x170;
    /// <summary>$82:D595/$D7A5: Power lower torso and Varia torso strips begin at $1A4/$182.</summary>
    internal const int PowerTorso = 0x1a4, VariaTorso = 0x182;
    /// <summary>$82:D771: Varia chest row begins at tile $1E0.</summary>
    internal const int VariaChest = 0x1e0;
    /// <summary>$82:D5B5/$D7D5: Power/Varia hip pieces begin at $1C4/$1CA.</summary>
    internal const int PowerHip = 0x1c4, VariaHip = 0x1ca;
    /// <summary>$82:D5D5/$D7F3: Power/Varia leg pieces begin at $1B6/$1E9.</summary>
    internal const int PowerLeg = 0x1b6, VariaLeg = 0x1e9;
    /// <summary>$82:D721 and Varia/Hi-Jump counterpart: boot toe piece begins at $198.</summary>
    internal const int HiJumpFoot = 0x198;
    /// <summary>$82:D555/D559: Power chest left/right strips start at $1EC/$17C.</summary>
    internal const int PowerChestLeft = 0x1ec, PowerChestRight = 0x17c;
    /// <summary>$82:D561: common lower-chest left strip starts at $1F0.</summary>
    internal const int LowerChestLeft = 0x1f0;
    /// <summary>$82:D565/$D785: Power/Varia lower-chest center strips start at $1FC/$1F2.</summary>
    internal const int PowerLowerChest = 0x1fc, VariaLowerChest = 0x1f2;
    /// <summary>$82:D58B: Power inner cannon strip starts at $178 and continues one packed row below.</summary>
    internal const int CannonInner = 0x178;
    /// <summary>$82:D58D: two-column outer cannon/arm strip starts at $186.</summary>
    internal const int CannonOuter = 0x186;
    /// <summary>$82:D72D: Hi-Jump outer foot starts at $19C and continues at $1AC.</summary>
    internal const int HiJumpOuterFoot = 0x19c;
    /// <summary>$82:D815/D825: regular Varia ankle strip uses $17B/$18B.</summary>
    internal const int VariaAnkle = 0x17b;
    /// <summary>$82:D703/D923: Hi-Jump collar starts at $179 and advances a packed row to $189.</summary>
    internal const int HiJumpCollar = 0x179;
    /// <summary>$82:D613/D615: two-character regular-foot upper strip starts at $19E.</summary>
    internal const int RegularFootUpper = 0x19e;
    /// <summary>$82:D621-D625: three-character regular toe strip starts at $1AD.</summary>
    internal const int RegularFootToe = 0x1ad;
    /// <summary>$82:D61D/D62D: outer regular foot strip uses $1EE/$1FE.</summary>
    internal const int RegularFootOuter = 0x1ee;
    /// <summary>Common wireframe pieces use palette one with priority; StockTile expresses the chosen layering.</summary>
    internal const int CommonPiecePalette = 1;

    /// <summary>$82:D5B3: hand tip at the end of the common arm strip.</summary>
    internal const int HandTip = 0x1c9;
    /// <summary>$82:D605: regular Power ankle joins the shin to the foot.</summary>
    internal const int RegularPowerAnkle = 0x18f;
    /// <summary>$82:D611: regular boot's outer toe edge.</summary>
    internal const int RegularToeEdge = 0x18e;
    /// <summary>$82:D705: inner Power/Hi-Jump collar.</summary>
    internal const int PowerHiJumpInnerCollar = 0x18c;
    /// <summary>$82:D60F and counterparts: diagonal equipment-guide connector.</summary>
    internal const int DiagonalConnector = 0x155;
}
