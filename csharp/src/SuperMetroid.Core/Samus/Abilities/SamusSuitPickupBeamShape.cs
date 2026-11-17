namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Mutually exclusive outlines the suit-pickup light beam takes across its stages.</summary>
public enum SamusSuitPickupBeamShape
{
    /// <summary>No lit row: the setup's updates before the beam grows leave its window empty.</summary>
    None,
    /// <summary>The narrow beam between <see cref="SamusSuitPickupBeam.Left"/> and <see cref="SamusSuitPickupBeam.Right"/> grows inward from the top and bottom edges by <see cref="SamusSuitPickupBeam.Progress"/> rows each.</summary>
    GrowingFromEdges,
    /// <summary>A uniform column from <see cref="SamusSuitPickupBeam.Left"/> to <see cref="SamusSuitPickupBeam.Right"/> on every row.</summary>
    Column,
    /// <summary>The column widened on each row by the authored curve, widest at the vertical center.</summary>
    CurvedColumn,
    /// <summary>The final curved column with <see cref="SamusSuitPickupBeam.Progress"/> rows wiped from the top and one fewer from the bottom.</summary>
    ClosingCurvedColumn,
    /// <summary>Only the center row remains lit, between <see cref="SamusSuitPickupBeam.Left"/> and <see cref="SamusSuitPickupBeam.Right"/>.</summary>
    CenterLine,
}
