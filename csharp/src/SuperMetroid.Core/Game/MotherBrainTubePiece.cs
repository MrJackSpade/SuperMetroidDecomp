namespace SuperMetroid.Core.Game;

/// <summary>
/// The five glass-tube pieces that fall when Mother Brain's tank collapses, in the order of their bank-$A9
/// population records ($A9:8AE5, sixteen bytes each). Each piece also has its own still-pose program.
/// </summary>
public enum MotherBrainTubePiece
{
    /// <summary>The bottom-left outer piece.</summary>
    BottomLeft,
    /// <summary>The bottom-right outer piece.</summary>
    BottomRight,
    /// <summary>The bottom-middle-left inner piece.</summary>
    BottomMiddleLeft,
    /// <summary>The bottom-middle-right inner piece.</summary>
    BottomMiddleRight,
    /// <summary>The main tube, which waits before falling.</summary>
    Main,
}
