namespace SuperMetroid.Core.Assets;

/// <summary>Stable resource names, extraction sources and native gameplay-HUD layout identities.</summary>
public static class GameplayHudDefinitions
{
    public const int Version = 1;
    public const string FileName = "gameplay-hud.json";
    public const int Width = 32;
    public const int Height = 3;
    public const int CellCount = Width * Height;

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

    /// <summary>Native byte offsets from WRAM <c>$7E:C608</c> for fourteen energy tanks.</summary>
    public static ReadOnlySpan<ushort> EnergyTankByteOffsets =>
    [
        0x42, 0x44, 0x46, 0x48, 0x4a, 0x4c, 0x4e,
        0x02, 0x04, 0x06, 0x08, 0x0a, 0x0c, 0x0e,
    ];

    /// <summary>Native byte offsets for Missile, Super, Power Bomb, Grapple and X-Ray icons.</summary>
    public static ReadOnlySpan<ushort> ItemByteOffsets => [0x14, 0x1c, 0x22, 0x28, 0x2e];

    /// <summary>Native mutable-row indexes for the six vertically repeated AUTO cells.</summary>
    public static ReadOnlySpan<int> AutoReserveCellIndices => [8, 9, 40, 41, 72, 73];

    public static string IconName(int itemIndex) => (uint)itemIndex < IconNames.Length
        ? IconNames[itemIndex]
        : throw new ArgumentOutOfRangeException(nameof(itemIndex));
}
