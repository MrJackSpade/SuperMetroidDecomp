using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A head's frames at each stage of opening its mouth, from closed to widest.</summary>
/// <param name="Closed">The closed mouth.</param>
/// <param name="Opening">The mouth starting to open.</param>
/// <param name="Open">The open mouth.</param>
/// <param name="Wide">The wide-open mouth.</param>
/// <param name="Widest">The widest-open mouth.</param>
internal sealed record MotherBrainHeadMouth(EnemyFrameId Closed, EnemyFrameId Opening, EnemyFrameId Open, EnemyFrameId Wide, EnemyFrameId Widest)
{
    /// <summary>The whole head of phase two.</summary>
    internal static MotherBrainHeadMouth Whole { get; } = new(EnemyFrameId.MotherBrainHeadEyeShut, EnemyFrameId.MotherBrainHeadEyeOpen,
        EnemyFrameId.MotherBrainHeadEyeGlaring, EnemyFrameId.MotherBrainHeadJawDropping, EnemyFrameId.MotherBrainHeadJawDropped);

    /// <summary>The damaged head of phase three.</summary>
    internal static MotherBrainHeadMouth Damaged { get; } = new(EnemyFrameId.MotherBrainDarkHeadEyeShut, EnemyFrameId.MotherBrainDarkHeadEyeOpen,
        EnemyFrameId.MotherBrainDarkHeadEyeGlaring, EnemyFrameId.MotherBrainDarkHeadJawDropping, EnemyFrameId.MotherBrainDarkHeadJawDropped);
}
