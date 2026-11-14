using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The five fixed enemy placements spawned during Mother Brain's
/// glass-tube collapse. They are separate from room population lists because
/// the cutscene selects one entry at a time after the initial room load.
/// </summary>
internal static class MotherBrainFallingTubePopulationDefinitions
{
    /// <summary>The five pieces in the order their spawn parameters and frames count them.</summary>
    internal static readonly IdentitySequence<MotherBrainTubePiece> Pieces = new(
        MotherBrainTubePiece.BottomLeft, MotherBrainTubePiece.BottomRight, MotherBrainTubePiece.BottomMiddleLeft,
        MotherBrainTubePiece.BottomMiddleRight, MotherBrainTubePiece.Main);

    /// <summary>Selected center X for the main tube; remains required.</summary>
    internal const int CenterX = 128;
    /// <summary>Chosen outside-piece horizontal distance from the center; remains required.</summary>
    internal const int OuterOffset = 32;
    /// <summary>Chosen inside-piece horizontal distance from the center; remains required.</summary>
    internal const int InnerOffset = 24;
    /// <summary>The five compositions share exclusive bottom Y215.
    /// This chosen placement baseline remains required, independent of installed artwork edits.</summary>
    internal const int BottomBaseline = 215;
    /// <summary>Outer tube compositions reach y28+8=36 below their origin.
    /// Geometry input remains required; not inferred from an editable presentation instance.</summary>
    internal const int OuterBottom = 36;
    /// <summary>Inner tube compositions reach y20+8=28 below their origin; required geometry.</summary>
    internal const int InnerBottom = 28;
    /// <summary>Main tube composition reaches y40+8=48 below its origin; required geometry.</summary>
    internal const int MainBottom = 48;
    /// <summary>Main-tube delay decremented before falling; remains required.</summary>
    internal const int MainFallDelay = 32;
}
