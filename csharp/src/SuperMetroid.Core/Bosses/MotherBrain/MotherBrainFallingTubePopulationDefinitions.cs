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

    /// <summary>
    /// Where each piece is placed as it falls: the main tube at the room's centre, each outer piece 32 pixels and each inner
    /// piece 24 pixels to its side of the centre, every piece's picture ending on the same row, 215.
    /// </summary>
    internal static readonly IReadOnlyDictionary<MotherBrainTubePiece, Position16<RoomPixels>> Positions =
        new Dictionary<MotherBrainTubePiece, Position16<RoomPixels>>
        {
            [MotherBrainTubePiece.BottomLeft] = new(96, 179),
            [MotherBrainTubePiece.BottomRight] = new(160, 179),
            [MotherBrainTubePiece.BottomMiddleLeft] = new(104, 187),
            [MotherBrainTubePiece.BottomMiddleRight] = new(152, 187),
            [MotherBrainTubePiece.Main] = new(128, 167),
        };

    /// <summary>Each piece's collision radius and the floor row it lands on.</summary>
    internal static readonly IReadOnlyDictionary<MotherBrainTubePiece, MotherBrainFallingTubeDefinition> Bodies =
        new Dictionary<MotherBrainTubePiece, MotherBrainFallingTubeDefinition>
        {
            [MotherBrainTubePiece.BottomLeft] = new(new(16, 32), 248),
            [MotherBrainTubePiece.BottomRight] = new(new(16, 32), 248),
            [MotherBrainTubePiece.BottomMiddleLeft] = new(new(8, 24), 240),
            [MotherBrainTubePiece.BottomMiddleRight] = new(new(8, 24), 240),
            [MotherBrainTubePiece.Main] = new(new(16, 32), 246),
        };
    /// <summary>Main-tube delay decremented before falling; remains required.</summary>
    internal const int MainFallDelay = 32;
}
