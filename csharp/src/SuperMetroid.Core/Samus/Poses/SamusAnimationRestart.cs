namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Restarts the animation at frame zero of the pose it animates (#1473). Unlike <see cref="SamusAnimationRepeat"/>,
/// whose target is a frame of its own program, a restart read past another pose's program still restarts that pose.
/// </summary>
internal sealed record SamusAnimationRestart : SamusAnimationInstruction;
