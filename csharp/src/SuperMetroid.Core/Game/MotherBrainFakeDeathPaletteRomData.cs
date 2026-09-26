namespace SuperMetroid.Core.Game;

/// <summary>Cartridge sources and destinations for Mother Brain's fake-death brain-only fade.</summary>
public static class MotherBrainFakeDeathPaletteRomData
{
    /// <summary>The native pointer lists each contain eight frames and a zero terminator.</summary>
    public const int FrameCount = 8;

    /// <summary>Eight grey-fade pointers followed by zero at $AD:ED8A.</summary>
    public const int ToGreyPointerTable = 0xaded8a;

    /// <summary>The resurrection uses the same $AD:ED9C pointer table as the later revival fade.</summary>
    public const int FromGreyPointerTable = MotherBrainDrainedPaletteRomData.FromGreyTable;

    /// <summary>The fake-death fade handlers copy three nontransparent brain sprite colors.</summary>
    public const int ColorCount = 3;

    /// <summary>The fade handlers write from CGRAM byte offset $0122.</summary>
    public const int BrainColor = 0x0122 / sizeof(ushort);
}
