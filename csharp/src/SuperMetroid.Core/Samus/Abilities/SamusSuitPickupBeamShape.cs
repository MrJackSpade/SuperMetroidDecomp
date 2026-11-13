namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Mutually exclusive outlines the suit-pickup light beam takes across its stages.</summary>
public enum SamusSuitPickupBeamShape
{
    /// <summary>No lit row: the setup calls before stage zero leave the beam window empty.</summary>
    None,
    /// <summary>Stage zero: the narrow beam between <see cref="SamusSuitPickupBeam.Left"/> and <see cref="SamusSuitPickupBeam.Right"/> grows inward from the top and bottom edges by <see cref="SamusSuitPickupBeam.Progress"/> rows each.</summary>
    GrowingFromEdges,
    /// <summary>Stage one: a uniform column from <see cref="SamusSuitPickupBeam.Left"/> to <see cref="SamusSuitPickupBeam.Right"/> on every row.</summary>
    Column,
    /// <summary>Stage two: the column widened on each row by the authored curve, widest at the vertical center.</summary>
    CurvedColumn,
    /// <summary>Stages three and four: the final curved column with <see cref="SamusSuitPickupBeam.Progress"/> rows wiped from the top and one fewer from the bottom.</summary>
    ClosingCurvedColumn,
    /// <summary>Stage five: only the center row remains lit, between <see cref="SamusSuitPickupBeam.Left"/> and <see cref="SamusSuitPickupBeam.Right"/>.</summary>
    CenterLine,
}
