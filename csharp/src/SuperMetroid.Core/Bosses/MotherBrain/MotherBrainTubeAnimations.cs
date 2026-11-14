using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The falling tubes' typed animations (#641, #1600).</summary>
internal static class MotherBrainTubeAnimations
{
    /// <summary>The pose each piece shows.</summary>
    private static readonly Dictionary<MotherBrainTubePiece, MotherBrainTubeFrame> PoseOf = new()
    {
        [MotherBrainTubePiece.BottomLeft] = MotherBrainTubeFrame.BottomLeft,
        [MotherBrainTubePiece.BottomRight] = MotherBrainTubeFrame.BottomRight,
        [MotherBrainTubePiece.BottomMiddleLeft] = MotherBrainTubeFrame.BottomMiddleLeft,
        [MotherBrainTubePiece.BottomMiddleRight] = MotherBrainTubeFrame.BottomMiddleRight,
        [MotherBrainTubePiece.Main] = MotherBrainTubeFrame.Main,
    };

    /// <summary>Every piece's segment: its pose for one update.</summary>
    internal static AnimationSegments<MotherBrainTubePiece, MotherBrainTubeFrame, NoAnimationCue> Segments { get; } = new(
        Enum.GetValues<MotherBrainTubePiece>().ToDictionary(piece => piece, piece => new AnimationBeat<MotherBrainTubeFrame, NoAnimationCue>[]
        {
            new AnimationBeat<MotherBrainTubeFrame, NoAnimationCue>.Frame(PoseOf[piece], 1),
        }));

    /// <summary>Each piece's animation: its pose, which the script's sleep then keeps.</summary>
    internal static IReadOnlyDictionary<MotherBrainTubePiece, Animation<MotherBrainTubePiece>> StartingAt { get; } =
        Enum.GetValues<MotherBrainTubePiece>().ToDictionary(piece => piece,
            piece => new Animation<MotherBrainTubePiece>([new(piece, 0, 1, 1)], AnimationEnd.Holds));

    /// <summary>The animation of <paramref name="piece"/>, before its first play.</summary>
    internal static AnimationPlayback<MotherBrainTubePiece> Starting(MotherBrainTubePiece piece) =>
        new(StartingAt[piece], AnimationPlace.StartOfAnimation);
}
