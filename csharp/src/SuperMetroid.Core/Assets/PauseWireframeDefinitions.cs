using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Four mutually exclusive visuals selected by the compiled Varia/Hi-Jump rule.</summary>
public enum PauseWireframeKind { PowerSuit, PowerSuitHiJump, VariaSuit, VariaSuitHiJump }

/// <summary>Native pause wireframe patch geometry, independent of inventory selection.</summary>
public static class PauseWireframeDefinitions
{
    public const int Version = 1;
    public const string FileName = "pause-wireframes.json";
    public const int Count = 4;
    /// <summary>$82:B25F Samus_Wireframe_Tilemaps pointer order: Power, Power/Hi-Jump, Varia, Varia/Hi-Jump.</summary>
    public const int Pointers = PauseMenuRomData.EquipmentTilemapPatchPointerTable;
    /// <summary>$82:B20C copies seventeen rows of eight words from bank $82.</summary>
    public const int Columns = 8, Rows = 17, Cells = Columns * Rows;
    /// <summary>$82:B20C destination starts at EquipmentScreenBG1Tilemap+$1D8, tile (12,7).</summary>
    public const int DestinationByte = 472;
    /// <summary>Native equipment page row stride in bytes.</summary>
    public const int DestinationStride = 64;
    /// <summary>Native equipment page byte size, one 32x32 tilemap.</summary>
    public const int DestinationSize = 2048;
    /// <summary>All four native artwork patches share bank $82.</summary>
    public const int Bank = 0x820000;
    /// <summary>Native packed body-piece rows advance sixteen BG characters within the pause interface artwork.</summary>
    private const int PieceRowStride = 16;
    /// <summary>$82:D527 and corresponding variants: left helmet half starts at tile $1B3.</summary>
    private const int Helmet = 0x1b3;
    /// <summary>$82:D531/$D751: Power/Varia shoulder pieces begin at $1BC/$1C0.</summary>
    private const int PowerShoulder = 0x1bc, VariaShoulder = 0x1c0;
    /// <summary>$82:D571 and corresponding variants: common left arm strip starts at $170.</summary>
    private const int Arm = 0x170;
    /// <summary>$82:D595/$D7A5: Power lower torso and Varia torso strips begin at $1A4/$182.</summary>
    private const int PowerTorso = 0x1a4, VariaTorso = 0x182;
    /// <summary>$82:D771: Varia chest row begins at tile $1E0.</summary>
    private const int VariaChest = 0x1e0;
    /// <summary>$82:D5B5/$D7D5: Power/Varia hip pieces begin at $1C4/$1CA.</summary>
    private const int PowerHip = 0x1c4, VariaHip = 0x1ca;
    /// <summary>$82:D5D5/$D7F3: Power/Varia leg pieces begin at $1B6/$1E9.</summary>
    private const int PowerLeg = 0x1b6, VariaLeg = 0x1e9;
    /// <summary>$82:D721 and Varia/Hi-Jump counterpart: boot toe piece begins at $198.</summary>
    private const int HiJumpFoot = 0x198;
    /// <summary>$82:D555/D559: Power chest left/right strips start at $1EC/$17C.</summary>
    private const int PowerChestLeft = 0x1ec, PowerChestRight = 0x17c;
    /// <summary>$82:D561: common lower-chest left strip starts at $1F0.</summary>
    private const int LowerChestLeft = 0x1f0;
    /// <summary>$82:D565/$D785: Power/Varia lower-chest center strips start at $1FC/$1F2.</summary>
    private const int PowerLowerChest = 0x1fc, VariaLowerChest = 0x1f2;
    /// <summary>$82:D58B: Power inner cannon strip starts at $178 and continues one packed row below.</summary>
    private const int CannonInner = 0x178;
    /// <summary>$82:D58D: two-column outer cannon/arm strip starts at $186.</summary>
    private const int CannonOuter = 0x186;
    /// <summary>$82:D72D: Hi-Jump outer foot starts at $19C and continues at $1AC.</summary>
    private const int HiJumpOuterFoot = 0x19c;
    /// <summary>$82:D815/D825: regular Varia ankle strip uses $17B/$18B.</summary>
    private const int VariaAnkle = 0x17b;
    /// <summary>$82:D703/D923: Hi-Jump collar starts at $179 and advances a packed row to $189.</summary>
    private const int HiJumpCollar = 0x179;
    /// <summary>$82:D613/D615: two-character regular-foot upper strip starts at $19E.</summary>
    private const int RegularFootUpper = 0x19e;
    /// <summary>$82:D621-D625: three-character regular toe strip starts at $1AD.</summary>
    private const int RegularFootToe = 0x1ad;
    /// <summary>$82:D61D/D62D: outer regular foot strip uses $1EE/$1FE.</summary>
    private const int RegularFootOuter = 0x1ee;
    /// <summary>Common native wireframe words use BG palette one with priority; deviations remain independent input.</summary>
    internal const ushort CommonPieceAttributes = 0x2400;

    /// <summary>Calculate tile references within identified rectangular body pieces.</summary>
    /// <remarks>Chosen piece origins/placements, uncovered artwork and priority differences
    /// remain required under PauseWireframePresentation.frames. This only removes the
    /// repeated within-piece tile progression, not the independent composition obligation.</remarks>
    internal static bool TryStockTile(PauseWireframeKind kind, int cell, out int tile)
    {
        if ((uint)kind >= Count) throw new ArgumentOutOfRangeException(nameof(kind));
        if ((uint)cell >= Cells) throw new IndexOutOfRangeException();
        int row = cell / Columns, column = cell % Columns;
        bool varia = kind is PauseWireframeKind.VariaSuit or PauseWireframeKind.VariaSuitHiJump;
        bool hiJump = kind is PauseWireframeKind.PowerSuitHiJump or PauseWireframeKind.VariaSuitHiJump;
        if (row <= 2 && column == 3) tile = Helmet + row * PieceRowStride;
        else if ((row is >= 1 and <= 2 && column < 3) || (!varia && row == 3 && column < 2))
            tile = (varia ? VariaShoulder : PowerShoulder) + (row - 1) * PieceRowStride + column;
        else if ((row is >= 5 and <= 8 && column < 2) || (row == 5 && column < 5) || (row == 6 && column is >= 3 and <= 4))
            tile = Arm + (row - 5) * PieceRowStride + column;
        else if (varia && row == 3) tile = VariaChest + column;
        else if (!varia && row == 3 && column is 2 or 3) tile = PowerChestLeft + column - 2;
        else if (!varia && row == 3 && column >= 4) tile = PowerChestRight + column - 4;
        else if (row == 4 && column < 2) tile = LowerChestLeft + column;
        else if (row == 4 && column is 2 or 3) tile = (varia ? VariaLowerChest : PowerLowerChest) + column - 2;
        else if (row is 6 or 7 && column == 5)
            tile = (varia ? VariaTorso + column - 2 : CannonInner) + (row - 6) * PieceRowStride;
        else if (row is >= 6 and <= 8 && column >= 6) tile = CannonOuter + (row - 6) * PieceRowStride + column - 6;
        else if (varia && row is >= 6 and <= 8 && column is >= 2 and <= 3)
            tile = VariaTorso + (row - 6) * PieceRowStride + column - 2;
        else if (!varia && row is >= 7 and <= 8 && column is >= 2 and <= 3)
            tile = PowerTorso + (row - 7) * PieceRowStride + column - 2;
        else if (row is >= 9 and <= 10 && column is >= 2 and <= 3)
            tile = (varia ? VariaHip : PowerHip) + (row - 9) * PieceRowStride + column - 2;
        else if (varia && row is >= 11 and <= 12 && column is >= 1 and <= 3)
            tile = VariaLeg + (row - 11) * PieceRowStride + column - 1;
        else if (!varia && row is >= 11 and <= 13 && column is >= 2 and <= 3 &&
                 (row < 13 || !hiJump || column == 3))
            tile = PowerLeg + (row - 11) * PieceRowStride + column - 2;
        else if (hiJump && row is >= 15 and <= 16 && (column < 3 || (row == 15 && column == 3)))
            tile = HiJumpFoot + (row - 15) * PieceRowStride + column;
        else if (hiJump && row is >= 15 and <= 16 && column == 6)
            tile = HiJumpOuterFoot + (row - 15) * PieceRowStride;
        else if (varia && !hiJump && row is 13 or 14 && column == 2)
            tile = VariaAnkle + (row - 13) * PieceRowStride;
        else if (hiJump && row is 13 or 14 && column is 1 or 2 &&
                 (varia || row == 14 || column == 1))
            tile = HiJumpCollar + (row - 13) * PieceRowStride + column - 1;
        else if (!hiJump && row == 15 && column is 1 or 2) tile = RegularFootUpper + column - 1;
        else if (!hiJump && row == 16 && column < 3) tile = RegularFootToe + column;
        else if (!hiJump && row is 15 or 16 && column == 6)
            tile = RegularFootOuter + (row - 15) * PieceRowStride;
        else { tile = 0; return false; }
        return true;
    }
}
