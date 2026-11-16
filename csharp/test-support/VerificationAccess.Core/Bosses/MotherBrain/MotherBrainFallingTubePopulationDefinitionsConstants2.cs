using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values and helpers of <see cref="MotherBrainFallingTubePopulationDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainFallingTubePopulationDefinitionsConstants2
{
    /// <summary>The five pieces in the order their spawn parameters and frames count them.</summary>
    internal static readonly IdentitySequence<MotherBrainTubePiece> Pieces = new(
        MotherBrainTubePiece.BottomLeft, MotherBrainTubePiece.BottomRight, MotherBrainTubePiece.BottomMiddleLeft,
        MotherBrainTubePiece.BottomMiddleRight, MotherBrainTubePiece.Main);
}
