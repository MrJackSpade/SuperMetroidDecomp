namespace SuperMetroid.Core.Assets;

/// <summary>Stable resource names, extraction sources and native gameplay-HUD layout identities.</summary>
public static class GameplayHudDefinitions
{
    public const int Version = 2;
    public const string FileName = "gameplay-hud.json";
    public const int Width = 32;
    public const int Height = 3;
    public const int CellCount = Width * Height;
    /// <summary>Immutable top row copied directly to BG3 from $80:988B.</summary>
    public const int TopRowAddress = 0x80988b;
    public const int TopRowCellCount = Width;
    public const int TopRowByteCount = TopRowCellCount * sizeof(ushort);

    /// <summary>The three mutable HUD rows copied from <c>$80:98CB</c>.</summary>
    public const int TemplateAddress = 0x8098cb;

    /// <summary>Missile, Super Missile, Power Bomb, Grapple and X-Ray icon cells at <c>$80:99A3</c>.</summary>
    public const int IconTableAddress = 0x8099a3;

    /// <summary>The ten health-counter character words at <c>$80:9DBF</c>.</summary>
    public const int HealthDigitsAddress = 0x809dbf;

    /// <summary>The ten ammunition-counter character words at <c>$80:9DD3</c>.</summary>
    public const int AmmoDigitsAddress = 0x809dd3;

    /// <summary>The six filled and six empty AUTO indicator words at <c>$80:998B</c>.</summary>
    public const int AutoReserveTableAddress = 0x80998b;

    /// <summary>The canonical blank HUD word used by native icon guards and MANUAL clearing.</summary>
    public const ushort BlankWord = 0x2c0f;

    /// <summary>The filled energy-tank word written by <c>$80:9BCF</c>.</summary>
    public const ushort FilledEnergyTankWord = 0x2831;

    /// <summary>The empty energy-tank word written by <c>$80:9BCF</c>.</summary>
    public const ushort EmptyEnergyTankWord = 0x3430;

    public const int SelectedPalette = 4;
    public const int DeselectedPalette = 5;

    public static readonly string[] IconNames = ["Missile", "SuperMissile", "PowerBomb", "Grapple", "XRay"];

    public const int EnergyTankCount = 14;
    public const int ItemCount = 5;
    public const int AutoReserveCellCount = 6;

    /// <summary><c>HandleHUDTilemap_PausedAndRunning.etankIconOffsets</c> at $80:9CCE: two seven-cell rows, bottom first.</summary>
    public static ushort EnergyTankByteOffset(int tank) => (uint)tank < EnergyTankCount
        ? (ushort)(2 * (1 + tank % 7 + (tank < 7 ? Width : 0)))
        : throw new IndexOutOfRangeException();

    /// <summary><c>ToggleHUDItemHighlight.HUDItemOffsets</c> at $80:9D6E: three-cell missiles, then two-cell icons, each with a blank gap.</summary>
    public static ushort ItemByteOffset(int item) => (uint)item < ItemCount
        ? (ushort)(2 * (10 + item * 3 + (item > 0 ? 1 : 0)))
        : throw new IndexOutOfRangeException();

    /// <summary>Native AUTO stores at $80:9B64..9B87 occupy columns eight/nine across all three mutable HUD rows.</summary>
    public static int AutoReserveCellIndex(int cell) => (uint)cell < AutoReserveCellCount
        ? 8 + cell % 2 + cell / 2 * Width
        : throw new IndexOutOfRangeException();
    /// <summary>Both HUD digit rows at $80:9DBF/$9DD3 use palette three, priority, and glyphs 1..9 followed by zero.</summary>
    internal static ushort DigitWord(int digit) => (uint)digit < 10
        ? (ushort)(0x2c00 | (digit + 9) % 10)
        : throw new IndexOutOfRangeException();
    /// <summary>
    /// $80:998B/9997 AUTO cells: the bottom row vertically reflects the top row
    /// (bit15); the empty indicator toggles palette bit12. Four full-state glyph/style
    /// inputs remain independent required payload; this derives the other eight cells.
    /// </summary>
    internal static ushort AutoReserveWord(ReadOnlySpan<ushort> basis, int cell, bool containsEnergy)
    {
        if ((uint)cell >= AutoReserveCellCount) throw new IndexOutOfRangeException();
        return (ushort)(basis[cell < 4 ? cell : cell - 4]
            ^ (cell >= 4 ? 0x8000 : 0) ^ (containsEnergy ? 0 : 0x1000));
    }
    public static string IconName(int itemIndex) => (uint)itemIndex < IconNames.Length
        ? IconNames[itemIndex]
        : throw new ArgumentOutOfRangeException(nameof(itemIndex));
}
