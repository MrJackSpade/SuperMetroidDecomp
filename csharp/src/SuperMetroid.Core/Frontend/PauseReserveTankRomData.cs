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
    /// <summary>$82:B3FC/$B433 always selects OBJ palette three, even as its unused timer advances.</summary>
    public const ushort PaletteBits = 0x0600;
}

/// <summary>The ten bank-$82 menu spritemaps the reserve-tank strip draws, valued by their $82:C569 ordinal.</summary>
public enum PauseReserveTankVisual : ushort
{
    /// <summary>$82:B305 full-tank spritemap ID.</summary>
    Full = 0x1b,
    /// <summary>$82:B396 final end-cap spritemap ID.</summary>
    EndCap = 0x1f,
    /// <summary>$82:B37D empty-tank spritemap ID.</summary>
    Empty = 0x20,
    /// <summary>Partial tank, one fill step.</summary>
    Fill1 = 0x21,
    /// <summary>Partial tank, two fill steps.</summary>
    Fill2 = 0x22,
    /// <summary>Partial tank, three fill steps.</summary>
    Fill3 = 0x23,
    /// <summary>Partial tank, four fill steps.</summary>
    Fill4 = 0x24,
    /// <summary>Partial tank, five fill steps.</summary>
    Fill5 = 0x25,
    /// <summary>Partial tank, six fill steps.</summary>
    Fill6 = 0x26,
    /// <summary>Partial tank, seven fill steps; drawn with the full-tank tile.</summary>
    Fill7 = 0x27,
}
