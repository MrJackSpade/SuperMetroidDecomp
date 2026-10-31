namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Spin-jump animation frames that wall contact rewinds to at <c>$90:9D96-$90:9DA6</c>.</summary>
internal static class SamusWallContactAnimationDefinitions
{
    /// <summary>
    /// Screw Attack's rewind frame: its longer pre-contact animation makes frame 27 the first
    /// eligible wall frame, so contact rewinds to 26.
    /// </summary>
    internal const ushort ScrewAttackRewindFrame = 26;

    /// <summary>Ordinary spin and Space Jump's rewind frame, the compact 10 -> 11 handoff.</summary>
    internal const ushort SpinJumpRewindFrame = 10;
}
