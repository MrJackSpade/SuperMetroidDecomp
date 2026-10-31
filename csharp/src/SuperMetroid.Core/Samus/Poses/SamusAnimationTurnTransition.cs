namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Publishes <paramref name="Pose"/> as the transitional pose that ends a turn or a landing, unless the pose input has
/// already selected a jump, which then supersedes it; an unsuperseded transition also latches the pending auto-jump input.
/// </summary>
/// <param name="Pose">The pose the turn or landing ends in.</param>
internal sealed record SamusAnimationTurnTransition(SamusPoseId Pose) : SamusAnimationInstruction;
