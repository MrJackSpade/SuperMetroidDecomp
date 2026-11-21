using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The falling tubes' typed animations (#641, #1600), each beat naming the installed frame it shows (#2249).</summary>
internal static class MotherBrainTubeAnimations
{
    /// <summary>Every piece's segment: its picture, held.</summary>
    internal static AnimationSegments<MotherBrainTubePiece, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MotherBrainTubePiece, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [MotherBrainTubePiece.BottomLeft] = [Show(EnemyFrameId.MotherBrainTubeBendRight)],
        [MotherBrainTubePiece.BottomRight] = [Show(EnemyFrameId.MotherBrainTubeBendLeft)],
        [MotherBrainTubePiece.BottomMiddleLeft] = [Show(EnemyFrameId.MotherBrainTubeStraightLeft)],
        [MotherBrainTubePiece.BottomMiddleRight] = [Show(EnemyFrameId.MotherBrainTubeStraightRight)],
        [MotherBrainTubePiece.Main] = [Show(EnemyFrameId.MotherBrainTubeColumn)],
    });

    /// <summary>Each piece's animation: its pose, which it then keeps.</summary>
    internal static Dictionary<MotherBrainTubePiece, Animation<MotherBrainTubePiece>> StartingAt { get; } =
        Enum.GetValues<MotherBrainTubePiece>().ToDictionary(piece => piece,
            piece => new Animation<MotherBrainTubePiece>([Segments.Whole(piece)], AnimationEnd.Holds));

    /// <summary>The animation of <paramref name="piece"/>, before its first play.</summary>
    internal static AnimationPlayback<MotherBrainTubePiece> Starting(MotherBrainTubePiece piece) =>
        new(StartingAt[piece], AnimationPlace.StartOfAnimation);

    /// <summary>A still picture (#2250): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame) =>
        new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}
