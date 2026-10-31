namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Publishes <paramref name="Pose"/> as the transitional pose Samus takes once the animation ends.</summary>
/// <param name="Pose">The pose the animation ends in.</param>
internal sealed record SamusAnimationTransition(SamusPoseId Pose) : SamusAnimationInstruction;
