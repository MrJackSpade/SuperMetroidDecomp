namespace SuperMetroid.Core.Frontend;

/// <summary>Native bank-$82 reserve tank strip definitions.</summary>
internal static class PauseReserveTankRomData
{
    /// <summary>$82:C1D6, tank and end-cap X origins.</summary>
    public const int XPositions = 0x82c1d6;
    /// <summary>$82:C1E2, Y origin plus one (draw routine decrements it).</summary>
    public const int YPosition = 0x82c1e2;
    /// <summary>$82:B3D9, partial tank spritemap IDs; duplicated for supplies at least 100.</summary>
    public const int PartialMaps = 0x82b3d9;
    /// <summary>$82:B350 adds sixteen bytes to select the second partial-map table.</summary>
    public const int SecondTableOffset = 16;
    /// <summary>$82:B2C0/$B2DB divides capacity/supply into 100-energy tanks.</summary>
    public const int EnergyPerTank = 100;
    /// <summary>$82:B31F divides the partial tank by fourteen.</summary>
    public const int EnergyPerFillStep = 14;
    /// <summary>$82:B332 compares the doubled fill quotient against seven.</summary>
    public const int FlickerDoubledQuotientLimit = 7;
    /// <summary>$82:B33F tests NMI counter bit two for low-fill flicker.</summary>
    public const int FlickerFrameMask = 4;
    /// <summary>$82:B305 full-tank spritemap ID.</summary>
    public const ushort FullMap = 0x1b;
    /// <summary>$82:B37D empty-tank spritemap ID.</summary>
    public const ushort EmptyMap = 0x20;
    /// <summary>$82:B396 final end-cap spritemap ID.</summary>
    public const ushort EndCapMap = 0x1f;
    /// <summary>$82:B3FC/$B433 always selects OBJ palette three, even as its unused timer advances.</summary>
    public const ushort PaletteBits = 0x0600;
}
