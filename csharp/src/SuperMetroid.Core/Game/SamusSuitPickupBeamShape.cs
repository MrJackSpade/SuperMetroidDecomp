namespace SuperMetroid.Core.Game;

/// <summary>Mutually exclusive outlines the suit-pickup light beam takes across its bank-$88 stages.</summary>
public enum SamusSuitPickupBeamShape : byte
{
    /// <summary>No lit scanline: the setup calls before stage zero leave the beam window empty.</summary>
    None,
    /// <summary>Stage zero, <c>$88:E092</c>: the narrow beam between <see cref="SamusSuitPickupBeam.Left"/> and <see cref="SamusSuitPickupBeam.Right"/> grows inward from the top and bottom edges by <see cref="SamusSuitPickupBeam.Progress"/> scanlines each.</summary>
    GrowingFromEdges,
    /// <summary>Stage one, <c>$88:E0D7</c>: a uniform column from <see cref="SamusSuitPickupBeam.Left"/> to <see cref="SamusSuitPickupBeam.Right"/> on every scanline.</summary>
    Column,
    /// <summary>Stage two, <c>$88:E113</c>: the column widened on each scanline by the authored curve, widest at the vertical center.</summary>
    CurvedColumn,
    /// <summary>Stages three and four, <c>$88:E1BA</c>: the final curved column with <see cref="SamusSuitPickupBeam.Progress"/> scanlines wiped from the top and one fewer from the bottom.</summary>
    ClosingCurvedColumn,
    /// <summary>Stage five, <c>$88:E22B</c>: only the center scanline remains lit, between <see cref="SamusSuitPickupBeam.Left"/> and <see cref="SamusSuitPickupBeam.Right"/>.</summary>
    CenterLine,
}
