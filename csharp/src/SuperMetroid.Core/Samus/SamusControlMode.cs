namespace SuperMetroid.Core.Samus;

/// <summary>How the two phases of Samus's frame run: her input mode and her movement mode.</summary>
/// <param name="Input">How the input phase runs, which control setups install.</param>
/// <param name="Movement">
/// How the movement phase runs: as installed, in <see cref="SamusControl.Mode"/>; as the frame runs it, in a frame's mode,
/// where the death states and the attract demo stand in for it.
/// </param>
internal readonly record struct SamusControlMode(SamusInputMode Input, SamusMovementMode Movement)
{
    /// <summary>The normal modes that restoring Samus's control installs.</summary>
    internal static SamusControlMode Normal => new(SamusInputMode.Normal, SamusMovementMode.Normal);
}
